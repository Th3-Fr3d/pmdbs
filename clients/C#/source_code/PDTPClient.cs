﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace pmdbs
{
    /// <summary>
    /// Handles the connection to a remote server.
    /// </summary>
    public struct PDTPClient
    {
        private static bool threadRunning = false;
        private static bool threadAbort = false;

        /// <summary>
        /// Checks whether a PDTPClient thread is currently running.
        /// </summary>
        public static bool ThreadRunning
        {
            get { return threadRunning; }
        }

        private static Mutex connectionMutex = new Mutex();
        /// <summary>
        /// Initializes the connection to a remote server using the PDTPS protocol
        /// </summary>
        public static async void Connect()
        {
            GlobalVarPool.ThreadIDs.Add(Thread.CurrentThread.ManagedThreadId);
            connectionMutex.WaitOne();
            if (threadAbort)
            {
                GlobalVarPool.ThreadIDs.Remove(Thread.CurrentThread.ManagedThreadId);
                if (GlobalVarPool.ThreadIDs.Count == 0)
                {
                    threadAbort = false;
                }
                connectionMutex.ReleaseMutex();
                return;
            }
            threadRunning = true;
            GlobalVarPool.connectionLost = false;
            if (string.IsNullOrEmpty(GlobalVarPool.PrivateKey) || string.IsNullOrEmpty(GlobalVarPool.PublicKey))
            {
                if (GlobalVarPool.USE_PERSISTENT_RSA_KEYS)
                {
                    DirectoryInfo d = new DirectoryInfo(Directory.GetCurrentDirectory());
                    bool retry = true;
                    WindowManager.LoadingScreen.InvokeSetStatus("Looking for RSA key pair in " + d.FullName + "\\keys ...");
                    if (!Directory.Exists(d.FullName + "\\keys"))
                    {
                        WindowManager.LoadingScreen.InvokeSetStatus("Directory \"keys\" does not exist in " + d.FullName + ".");
                        WindowManager.LoadingScreen.InvokeSetStatus("Creating directory \"keys\" ...");
                        try
                        {
                            Directory.CreateDirectory(d.FullName + "\\keys");
                        }
                        catch (Exception e)
                        {
                            AutomatedTaskFramework.Tasks.GetCurrentOrDefault()?.Terminate();
                            CustomException.ThrowNew.GenericException("Could not create directory: " + e.ToString());
                            return;
                        }
                        WindowManager.LoadingScreen.InvokeSetStatus("Directory \"keys\" created successfully.");
                    }
                    while (retry)
                    {
                        d = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\keys");
                        FileInfo[] files = d.GetFiles().Where(file => (new[] { "client.privatekey", "client.publickey" }).Contains(file.Name)).ToArray();
                        if (files.Length < 2)
                        {
                            bool selected = false;
                            while (!selected)
                            {
                                WindowManager.LoadingScreen.InvokeSetStatus("No key file found!");
                                WindowManager.LoadingScreen.InvokeSetStatus("Generating RSA Keys ...");
                                WindowManager.LoadingScreen.InvokeSetStatus("Generating 4096 bit RSA key pair...");
                                string[] RSAKeyPair = CryptoHelper.RSAKeyPairGenerator();
                                GlobalVarPool.PublicKey = RSAKeyPair[0];
                                GlobalVarPool.PrivateKey = RSAKeyPair[1];
                                WindowManager.LoadingScreen.InvokeSetStatus("Exporting RSA private key ...");
                                File.WriteAllText(d.FullName + "\\client.privatekey", GlobalVarPool.PrivateKey);
                                WindowManager.LoadingScreen.InvokeSetStatus("RSA private key exported successfully.");
                                WindowManager.LoadingScreen.InvokeSetStatus("Exporting RSA public key ...");
                                File.WriteAllText(d.FullName + "\\client.publickey", GlobalVarPool.PublicKey);
                                WindowManager.LoadingScreen.InvokeSetStatus("RSA public key exported successfully.");
                                WindowManager.LoadingScreen.InvokeSetStatus("Generated RSA key pair.");
                                retry = false;
                                selected = true;
                            }
                        }
                        else
                        {
                            WindowManager.LoadingScreen.InvokeSetStatus("Reading RSA keys ...");
                            // KINDA LAZY BUT IT WORKS
                            GlobalVarPool.PrivateKey = File.ReadAllText(files.Where(file => file.Name.Equals("client.privatekey")).ToArray()[0].FullName);
                            GlobalVarPool.PublicKey = File.ReadAllText(files.Where(file => file.Name.Equals("client.publickey")).ToArray()[0].FullName);
                            WindowManager.LoadingScreen.InvokeSetStatus("Successfully set up RSA keys.");
                            retry = false;
                        }
                    }
                }
                else
                {
                    WindowManager.LoadingScreen.InvokeSetStatus("Generating RSA keys ...");
                    WindowManager.LoadingScreen.InvokeSetStatus("Generating 4096 bit RSA key pair...");
                    string[] RSAKeyPair = CryptoHelper.RSAKeyPairGenerator();
                    GlobalVarPool.PublicKey = RSAKeyPair[0];
                    GlobalVarPool.PrivateKey = RSAKeyPair[1];
                    WindowManager.LoadingScreen.InvokeSetStatus("Generated RSA key pair.");
                    WindowManager.LoadingScreen.InvokeSetStatus("Successfully set up RSA keys.");
                }
            }
            if (string.IsNullOrEmpty(GlobalVarPool.cookie))
            {
                WindowManager.LoadingScreen.InvokeSetStatus("Looking for cookie ...");
                Task<string> GetCookie = DataBaseHelper.GetSingleOrDefault("SELECT U_cookie FROM Tbl_user;");
                GlobalVarPool.cookie = await GetCookie;
                if (string.IsNullOrEmpty(GlobalVarPool.cookie))
                {
                    WindowManager.LoadingScreen.InvokeSetStatus("Cookie not found.");
                }
                else
                {
                    WindowManager.LoadingScreen.InvokeSetStatus("Found cookie.");
                }
            }
            GlobalVarPool.threadKilled = false;
            string ip = GlobalVarPool.REMOTE_ADDRESS;
            int port = GlobalVarPool.REMOTE_PORT;
            WindowManager.LoadingScreen.InvokeSetStatus("Connecting to " + ip + ":" + port + " ...");
            bool isDisconnected = false;
            bool isSocketError = false;
            bool isTcpFin = false;
            IPAddress ipAddress;
            try
            {
                IPHostEntry entry = Dns.GetHostEntry(ip);
                ipAddress = entry.AddressList[0];
            }
            catch (Exception e)
            {
                CustomException.ThrowNew.NetworkException("Unable to resolve + " + ip + " Error message: " + e.ToString());
                GlobalVarPool.connectionLost = true;
                AutomatedTaskFramework.Tasks.GetCurrentOrDefault()?.Terminate();
                return;
            }
            IPEndPoint server = new IPEndPoint(ipAddress, port);
            GlobalVarPool.clientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                GlobalVarPool.clientSocket.Connect(server);
            }
            catch
            {
                //TODO: MOVE THIS CODE TO FAILED ACTION
                CustomException.ThrowNew.NetworkException("Could not connect to server:\n\nTimed out or connection refused.");
                MainForm.InvokeSyncAnimationStop();
                GlobalVarPool.MainForm.Invoke((System.Windows.Forms.MethodInvoker)delegate 
                {
                    GlobalVarPool.syncButton.Enabled = true;
                });
                AutomatedTaskFramework.Tasks.GetCurrentOrDefault()?.Terminate();
                GlobalVarPool.connectionLost = true;
                return;
            }
            ip = ipAddress.ToString();
            string address = ip + ":" + port;
            WindowManager.LoadingScreen.InvokeSetStatus("Successfully connected to " + ip + ":" + port + "!");
            GlobalVarPool.bootCompleted = true;
            GlobalVarPool.connected = true;
            WindowManager.LoadingScreen.InvokeSetStatus("Sending Client Hello ...");
            GlobalVarPool.clientSocket.Send(Encoding.UTF8.GetBytes("\x01UINICRT\x04"));
            // AWAIT PACKETS FROM SERVER
            try
            {
                
                // INITIALIZE BUFFER FOR HUGE PACKETS (>32 KB)
                List<byte> buffer = new List<byte>();
                // INITIALIZE 32 KB RECEIVE BUFFER FOR INCOMING DATA
                int bufferSize = 32768;
                byte[] data = new byte[bufferSize];

                // RUN UNTIL THREAD IS TERMINATED
                while (true)
                {
                    bool receiving = true;
                    // INITIALIZE LIST TO STORE ALL PACKETS FOUND IN RECEIVE BUFFER
                    List<byte[]> dataPackets = new List<byte[]>();
                    // RECEIVE AND DUMP TO BUFFER UNTIL EOT FLAG (USED TO TERMINATE PACKETS IN CUSTOM PROTOCOL --> HEX VALUE 0x04) IS FOUND
                    while (receiving)
                    {
                        // LOAD DATA TO 32768 BYTE BUFFER
                        int connectionDropped = GlobalVarPool.clientSocket.Receive(data);
                        // CHECK IF RECEIVED 0 BYTE MESSAGE (TCP RST)
                        if (connectionDropped == 0)
                        {
                            // BREAK ENDLESS LOOP AND JUMP TO CATCH
                            GlobalVarPool.threadKilled = true;
                            throw new SocketException { Source = "RST" };
                        }
                        // ----HANDLE CASES OF MORE THAN ONE PACKET IN RECEIVE BUFFER
                        // REMOVE ANY NULL BYTES FROM BUFFER
                        data = data.Where(b => b != 0x00).ToArray();
                        // CHECK IF PACKET CONTAINS EOT FLAG AND IF THE BUFFER FOR BIG PACKETS IS EMPTY
                        if (data.Contains<byte>(0x04) && buffer.Count == 0)
                        {
                            // SPLIT PACKETS ON EOT FLAG (MIGHT BE MORE THAN ONE PACKET)
                            List<byte[]> rawDataPackets = HelperMethods.Separate(data, new byte[] { 0x04 });
                            // GRAB THE LAST PACKET
                            byte[] lastDataPacket = rawDataPackets[rawDataPackets.Count - 1];
                            // MOVE ALL BUT THE LAST PACKET INTO THE 2D PACKET ARRAY LIST
                            List<byte[]> tempRawDataPackets = new List<byte[]>(rawDataPackets);
                            tempRawDataPackets.Remove(tempRawDataPackets.Last());
                            dataPackets = new List<byte[]>(tempRawDataPackets);
                            // IN CASE THE LAST PACKET CONTAINS DATA TOO --> MOVE IT IN BUFFER FOR NEXT "RECEIVING ROUND"
                            if (lastDataPacket.Length != 0 && lastDataPacket.Any(b => b != 0))
                            {
                                buffer.AddRange(new List<byte>(lastDataPacket));
                            }
                            // STOP RECEIVING AND BREAK THE LOOP
                            receiving = false;
                        }
                        // CHECK IF PACKET CONTAINS EOT FLAG AND THE BUFFER IS NOT EMPTY
                        else if (data.Contains<byte>(0x04) && buffer.Count != 0)
                        {
                            // SPLIT PACKETS ON EOT FLAG (MIGHT BE MORE THAN ONE PACKET)
                            List<byte[]> rawDataPackets = HelperMethods.Separate(data, new byte[] { 0x04 });
                            // APPEND CONTENT OF BUFFER TO THE FIRST PACKET
                            List<byte> firstPacket = new List<byte>();
                            firstPacket.AddRange(buffer);
                            firstPacket.AddRange(new List<byte>(rawDataPackets[0]));
                            rawDataPackets[0] = firstPacket.ToArray();
                            // RESET THE BUFFER
                            buffer = new List<byte>();
                            // GRAB THE LAST PACKET
                            byte[] lastDataPacket = rawDataPackets[rawDataPackets.Count - 1];
                            // MOVE ALL BUT THE LAST PACKET INTO THE 2D PACKET ARRAY LIST
                            List<byte[]> tempRawDataPackets = new List<byte[]>(rawDataPackets);
                            tempRawDataPackets.Remove(tempRawDataPackets.Last());
                            dataPackets = new List<byte[]>(tempRawDataPackets);
                            // IN CASE THE LAST PACKET CONTAINS DATA TOO --> MOVE IT IN BUFFER FOR NEXT "RECEIVING ROUND"
                            if (lastDataPacket.Length != 0 && lastDataPacket.Any(b => b != 0))
                            {
                                buffer.AddRange(new List<byte>(lastDataPacket));
                            }
                            // STOP RECEIVING AND BREAK THE LOOP
                            receiving = false;
                        }
                        // THE BUFFER DOES NOT CONTAIN ANY EOT FLAG
                        else
                        {
                            // DAMN THAT'S A HUGE PACKET. APPEND THE WHOLE THING TO THE BUFFER AND REPEAT UNTIL EOT FLAG IS FOUND
                            buffer.AddRange(new List<byte>(data));
                        }
                        // RESET THE DATA BUFFER
                        data = new byte[bufferSize];
                    }
                    // ITERATE OVER EVERY SINGE PACKET THAT HAS BEEN RECEIVED
                    for (int i = 0; i < dataPackets.Count; i++)
                    {
                        // LOAD BYTE ARRAY INTO BYTE LIST --> EASIER TO HANDLE THAN BYTE ARRAYS
                        List<byte> dataPacket = new List<byte>(dataPackets[i]);
                        // CHECK IF PACKETS HAVE A VALID ENTRYPOINT / START OF HEADING
                        if (dataPacket[0] != 0x01)
                        {
                            // WELL... APPARENTLY THERE'S NO ENTRY POINT --> IGNORE AND CONTINUE WITH NEXT ONE
                            if (!dataPacket.Contains(0x01))
                            {
                                CustomException.ThrowNew.NetworkException("Received invalid packet from server.", "[ERRNO 02] ISOH");
                                continue;
                            }
                            // CHECK AND HOPE THAT THERE'S AT LEAST ONE VALID ENTRY POINT
                            else if (dataPacket.Where(currentByte => currentByte.Equals(0x01)).Count() == 1)
                            {
                                dataPacket.RemoveRange(0, dataPacket.IndexOf(0x01));
                            }
                            // THIS PACKET IS OFICIALLY BROKEN (CONTAINING SEVERAL ENTRY POINTS). HOPE THAT IT WASN'T TOO IMPORTANT AND CONTINUE WITH THE NEXT ONE
                            else
                            {
                                CustomException.ThrowNew.NetworkException("Received invalid packet from server.", "[ERRNO 02] ISOH");
                                continue;
                            }
                        }
                        // REMOVE ENTRY POINT MARKER BYTE (0x01)
                        dataPacket.RemoveAt(0);
                        // CONVERT THAT THING TO UTF-8 STRING
                        string dataString = Encoding.UTF8.GetString(dataPacket.ToArray());
                        // GRAB THE PACKET SPECIFIER --> INDICATES ENCRYPTION STATE
                        char packetSpecifier = dataString[0];
                        // FROM HERE ON USE MAIN PROTOCOL DEFINED HERE: https://github.com/Th3-Fr3d/pmdbs/blob/development/doc/CustomProtocolFinal.pdf
                        // ALL THAT'S HAPPENING FROM HERE ON DOWN IS PACKET PARSING AND MATCHING METHODS TO SAID PACKETS. SHOULD BE SOMEWHERE BETWEEN SELF-EXPLANATORY AND BLACK MAGIC
                        switch (packetSpecifier)
                        {
                            case 'U':
                                {
                                    string packetID = dataString.Substring(1, 3);
                                    switch (packetID)
                                    {
                                        case "FIN":
                                            {
                                                isDisconnected = true;
                                                try
                                                {
                                                    GlobalVarPool.clientSocket.Disconnect(true);
                                                    GlobalVarPool.clientSocket.Close();
                                                    GlobalVarPool.clientSocket.Dispose();
                                                    isTcpFin = true;
                                                }
                                                catch { }
                                                HelperMethods.Debug("PDTPClient:  Disconnected.");
                                                HelperMethods.Debug("PDTPClient:  REASON: " + dataString.Substring(4).Split(new string[] { "%eq" }, StringSplitOptions.None)[1].Replace("!", "").Replace(";", ""));
                                                
                                                return;
                                            }
                                        case "KEY":
                                            {
                                                WindowManager.LoadingScreen.InvokeSetStatus("Received Server Hello ...");
                                                string packetSID = dataString.Substring(4, 3);
                                                switch (packetSID)
                                                {
                                                    case "XML":
                                                        {
                                                            GlobalVarPool.foreignRsaKey = dataString.Substring(7).Split('!')[1];
                                                            GlobalVarPool.nonce = CryptoHelper.RandomString();
                                                            string encNonce = CryptoHelper.RSAEncrypt(GlobalVarPool.foreignRsaKey, GlobalVarPool.nonce);
                                                            string message = "CKEkey%eq!" + GlobalVarPool.PublicKey + "!;nonce%eq!" + encNonce + "!;";
                                                            WindowManager.LoadingScreen.InvokeSetStatus("Client Key Exchange ...");
                                                            GlobalVarPool.clientSocket.Send(Encoding.UTF8.GetBytes("\x01K" + message + "\x04"));
                                                            break;
                                                        }
                                                    case "CRT":
                                                        {
                                                            CryptoHelper.CertificateInformation certificate = CryptoHelper.CreateCertificateFromString(dataString.Substring(7).Split('!')[1]);
                                                            Task<string> GetTrustedCertificates = DataBaseHelper.GetSingleOrDefault(DataBaseHelper.Security.SQLInjectionCheckQuery(new string[] { "SELECT EXISTS (SELECT 1 FROM Tbl_certificates WHERE C_hash = \"", certificate.Checksum, "\" AND C_accepted = \"1\" LIMIT 1);"}));
                                                            string isTrustedCertificateString = await GetTrustedCertificates;
                                                            if (!isTrustedCertificateString.Equals("1"))
                                                            {
                                                                WindowManager.ShowCertificateWarning(certificate);
                                                            }
                                                            else
                                                            {
                                                                GlobalVarPool.foreignRsaKey = certificate.PublicKey;
                                                                GlobalVarPool.nonce = CryptoHelper.RandomString();
                                                                string encNonce = CryptoHelper.RSAEncrypt(GlobalVarPool.foreignRsaKey, GlobalVarPool.nonce);
                                                                string message = "CKEformat%eq!XML!;key%eq!" + GlobalVarPool.PublicKey + "!;nonce%eq!" + encNonce + "!;";
                                                                WindowManager.LoadingScreen.InvokeSetStatus("Client Key Exchange ...");
                                                                GlobalVarPool.clientSocket.Send(Encoding.UTF8.GetBytes("\x01K" + message + "\x04"));
                                                            }
                                                            break;
                                                        }
                                                    case "ERR":
                                                        {
                                                            GlobalVarPool.clientSocket.Send(Encoding.UTF8.GetBytes("\x01UINIXML\x04"));
                                                            break;
                                                        }
                                                    default:
                                                        {
                                                            CustomException.ThrowNew.CryptographicException("RSA Key Format not supported.");
                                                            break;
                                                        }
                                                }
                                                break;
                                            }
                                        default:
                                            {
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case 'K':
                                {
                                    string decrypted = CryptoHelper.RSADecrypt(GlobalVarPool.PrivateKey, dataString.Substring(1));
                                    string packetID = decrypted.Substring(0, 3);
                                    if (packetID.Equals("SKE"))
                                    {
                                        WindowManager.LoadingScreen.InvokeSetStatus("Symmetric Keys Exchange ...");
                                        string key = string.Empty;
                                        string providedNonce = string.Empty;
                                        foreach (string returnValue in decrypted.Substring(3).Split(';'))
                                        {
                                            if (returnValue.Contains("key"))
                                            {
                                                key = returnValue.Split('!')[1];
                                            }
                                            else if (returnValue.Contains("nonce"))
                                            {
                                                providedNonce = returnValue.Split('!')[1].Replace("\0", "");
                                            }
                                        }
                                        HelperMethods.Debug("PDTPClient:  Provided nonce: " + providedNonce);
                                        HelperMethods.Debug("PDTPClient:  Real nonce: " + GlobalVarPool.nonce);
                                        if (!providedNonce.Equals(GlobalVarPool.nonce))
                                        {
                                            return;
                                        }
                                        HelperMethods.Debug("PDTPClient:  Nonce ok!");
                                        GlobalVarPool.aesKey = key;
                                        string shaIn = GlobalVarPool.aesKey + GlobalVarPool.nonce;
                                        GlobalVarPool.hmac = CryptoHelper.SHA256Hash(GlobalVarPool.aesKey + GlobalVarPool.nonce);
                                        HelperMethods.Debug("PDTPClient:  AES KEY: " + GlobalVarPool.aesKey);
                                        HelperMethods.Debug("PDTPClient:  NONCE: " + GlobalVarPool.nonce);
                                        HelperMethods.Debug("PDTPClient:  HMAC KEY: " + GlobalVarPool.hmac);
                                        GlobalVarPool.nonce = CryptoHelper.RandomString();
                                        WindowManager.LoadingScreen.InvokeSetStatus("Acknowledging ...");
                                        Network.SendEncrypted("KEXACKnonce%eq!" + GlobalVarPool.nonce + "!;");
                                    }
                                    break;
                                }
                            case 'E':
                                {
                                    Thread.Sleep(100); // <-- APPARENTLY THERE'S SOME NASTY BUG SOMEWHERE. DONT KNOW DONT CARE. USING "Thread.Sleep(100);" SEEMS TO FIX IT SOMEHOW. JUST MICROSOFT THINGS *sigh*
                                    if (!CryptoHelper.VerifyHMAC(GlobalVarPool.hmac, dataString.Substring(1)))
                                    {
                                        CustomException.ThrowNew.NetworkException("Received an invalid HMAC checksum.", "[ERRNO 31] IMAC");
                                    }
                                    else
                                    {
                                        HelperMethods.Debug("PDTPClient:  HMAC ok!");
                                    }
                                    string decryptedData = CryptoHelper.AESDecrypt(dataString.Substring(1, dataString.Length - 45), GlobalVarPool.aesKey);
                                    HelperMethods.Debug("PDTPClient:  SERVER: " + decryptedData);
                                    string packetID = decryptedData.Substring(0, 3);
                                    string packetSID = decryptedData.Substring(3, 3);
                                    // AUTOMATED TASK MANAGEMENT (CHECK FOR COMPLETED TASKS AND START NEXT ONE IN QUEUE)
                                    AutomatedTaskFramework.DoNetworkTasks(decryptedData);
                                    switch (packetID)
                                    {
                                        case "KEX":
                                            {
                                                if (!packetSID.Equals("ACK") || !decryptedData.Substring(6).Split('!')[1].Equals(GlobalVarPool.nonce))
                                                {
                                                    AutomatedTaskFramework.Tasks.GetCurrentOrDefault()?.Terminate();
                                                    return;
                                                }
                                                else
                                                {
                                                    WindowManager.LoadingScreen.InvokeSetStatus("Key Exchange finished.");
                                                    if (GlobalVarPool.cookie.Equals(string.Empty))
                                                    {
                                                        NetworkAdapter.MethodProvider.GetCookie();
                                                    }
                                                    else
                                                    {
                                                        NetworkAdapter.MethodProvider.CheckCookie();
                                                    }
                                                    break;
                                                }
                                            }
                                        case "DTA":
                                            {
                                                switch (packetSID)
                                                {
                                                    case "LOG":
                                                        {
                                                            try
                                                            {
                                                                string[] parts = decryptedData.Substring(6).Split('§');
                                                                string mode = string.Empty;
                                                                string message = string.Empty;
                                                                foreach (string part in parts)
                                                                {
                                                                    if (part.Contains("mode"))
                                                                    {
                                                                        mode = part.Split('$')[1];
                                                                    }
                                                                    else if (part.Contains("msg"))
                                                                    {
                                                                        message = part.Split('$')[1];
                                                                    }
                                                                    else if (part.Length != 0)
                                                                    {
                                                                        CustomException.ThrowNew.FormatException("Unexpected number of arguments in log dump.");
                                                                    }
                                                                }
                                                                if (new string[] { mode, message }.Contains(string.Empty))
                                                                {
                                                                    CustomException.ThrowNew.FormatException("Too few arguments in log dump.");
                                                                }
                                                                else
                                                                {
                                                                    // TODO: ACTUALLY DISPLAY THESE LOGS
                                                                    if (mode.Equals("USER_REQUEST"))
                                                                    {
                                                                        //ConsoleExtension.PrintF("Your account activity:");
                                                                        //ConsoleExtension.PrintF(message);
                                                                    }
                                                                    //ConsoleExtension.PrintF("Received " + mode + " log.");
                                                                    //ConsoleExtension.PrintF(message);
                                                                }
                                                            }
                                                            catch (IndexOutOfRangeException e)
                                                            {
                                                                CustomException.ThrowNew.FormatException(e.ToString());
                                                            }
                                                            break;
                                                        }
                                                    case "RET":
                                                        {
                                                            // TODO: IMPLEMENT BETTER ERROR HANDLING
                                                            try
                                                            {
                                                                string content = decryptedData.Substring(6);
                                                                string mode = content.Split(';').Where(element => element.Contains("mode")).FirstOrDefault();
                                                                if (string.IsNullOrEmpty(mode))
                                                                {
                                                                    break;
                                                                }
                                                                switch (mode.Split('!')[1])
                                                                {
                                                                    // TODO: IMPLEMENT RETURN VALUE HANDLING
                                                                    case "INSERT":
                                                                        {
                                                                            Thread t = new Thread(new ParameterizedThreadStart(Sync.SetHid));
                                                                            t.Start((object)content.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries));
                                                                            if (GlobalVarPool.countSyncPackets)
                                                                            {
                                                                                GlobalVarPool.countedPackets++;
                                                                            }
                                                                            break;
                                                                        }
                                                                    case "FETCH_ALL":
                                                                        {
                                                                            break;
                                                                        }
                                                                    case "FETCH_SYNC":
                                                                        {
                                                                            string remoteHeaderString = content.Split(';').Where(element => element.Contains("headers")).FirstOrDefault();
                                                                            string deletedItemString = content.Split(';').Where(element => element.Contains("deleted")).FirstOrDefault();
                                                                            object parameters = new string[] { remoteHeaderString, deletedItemString };
                                                                            Thread t = new Thread(new ParameterizedThreadStart(Sync.Initialize));
                                                                            t.Start(parameters);
                                                                            GlobalVarPool.hidThreadCounter++;
                                                                            break;
                                                                        }
                                                                    case "DELETING_COMPLETED":
                                                                    case "UPDATE":
                                                                        {
                                                                            if (GlobalVarPool.countSyncPackets)
                                                                            {
                                                                                GlobalVarPool.countedPackets++;
                                                                            }
                                                                            break;
                                                                        }
                                                                    case "SELECT":
                                                                        {
                                                                            GlobalVarPool.selectedAccounts.Add(content);
                                                                            if (GlobalVarPool.countSyncPackets)
                                                                            {
                                                                                GlobalVarPool.countedPackets++;
                                                                            }
                                                                            break;
                                                                        }
                                                                    default:
                                                                        {
                                                                            break;
                                                                        }
                                                                }
                                                                if (GlobalVarPool.expectedPacketCount == GlobalVarPool.countedPackets && GlobalVarPool.countSyncPackets)
                                                                {
                                                                    GlobalVarPool.countSyncPackets = false;
                                                                    List<AutomatedTaskFramework.Task> scheduledTasks = AutomatedTaskFramework.Tasks.DeepCopy();
                                                                    AutomatedTaskFramework.Tasks.Clear();
                                                                    AutomatedTaskFramework.Task.Create(TaskType.NetworkTask, SearchCondition.In, "LOGGED_OUT|NOT_LOGGED_IN", NetworkAdapter.MethodProvider.Logout);
                                                                    AutomatedTaskFramework.Task.Create(TaskType.FireAndForget,  NetworkAdapter.MethodProvider.Disconnect);
                                                                    for (int j = 1; j < scheduledTasks.Count; j++)
                                                                    {
                                                                        AutomatedTaskFramework.Tasks.Schedule(scheduledTasks[j]);
                                                                    }
                                                                    AutomatedTaskFramework.Tasks.Execute();
                                                                    new Thread(new ThreadStart(Sync.Finish))
                                                                    {
                                                                        IsBackground = true
                                                                    }.Start();
                                                                }
                                                            }
                                                            catch (Exception e)
                                                            {
                                                                CustomException.ThrowNew.FormatException("Error in return handling:" + Environment.NewLine + e.ToString());
                                                            }
                                                            break;
                                                        }
                                                    case "CKI":
                                                        {
                                                            GlobalVarPool.cookie = decryptedData.Substring(6).Split('!')[1];
                                                            await DataBaseHelper.ModifyData(DataBaseHelper.Security.SQLInjectionCheckQuery(new string[] { "UPDATE Tbl_user SET U_cookie = \"", GlobalVarPool.cookie, "\";" }));
                                                            NetworkAdapter.MethodProvider.Authorize();
                                                            break;
                                                        }
                                                    case "DEV":
                                                        {
                                                            HelperMethods.LoadDevices(decryptedData.Substring(6).Split('!')[1]);
                                                            break;
                                                        }
                                                }
                                                break;
                                            }
                                        case "INF":
                                            {
                                                switch (packetSID)
                                                {
                                                    case "ERR":
                                                        {
                                                            string errno = string.Empty;
                                                            string errID = string.Empty;
                                                            string message = string.Empty;
                                                            try
                                                            {
                                                                errno = decryptedData.Split(';').Where(element => element.ToLower().Contains("errno")).ToArray()[0].Split('!')[1];
                                                                errID = decryptedData.Split(';').Where(element => element.ToLower().Contains("code")).ToArray()[0].Split('!')[1];
                                                                message = decryptedData.Split(';').Where(element => element.ToLower().Contains("message")).ToArray()[0].Split('!')[1];
                                                            }
                                                            catch
                                                            {
                                                                errno = "-1";
                                                                errID = "UNKN";
                                                                message = "UNKNOWN ERROR";
                                                            }
                                                            switch (errID)
                                                            {
                                                                case "UEXT":
                                                                    {
                                                                        AutomatedTaskFramework.Tasks.GetCurrentOrDefault()?.Terminate();
                                                                        CustomException.ThrowNew.GenericException("This username is already in use." + Environment.NewLine + message);
                                                                        break;
                                                                    }
                                                                case "MAIL":
                                                                    {
                                                                        if (message.Equals("EMAIL_ALREADY_IN_USE"))
                                                                        {
                                                                            CustomException.ThrowNew.GenericException("This email address is already in use." + Environment.NewLine + message);
                                                                        }
                                                                        break;
                                                                    }
                                                                case "UDNE":
                                                                    {
                                                                        CustomException.ThrowNew.GenericException("This username does not exist." + Environment.NewLine + message);
                                                                        break;
                                                                    }
                                                                case "CRED":
                                                                    {
                                                                        CustomException.ThrowNew.GenericException("Invalid credentials." + Environment.NewLine + message);
                                                                        AutomatedTaskFramework.Tasks.GetCurrentOrDefault()?.Terminate();
                                                                        break;
                                                                    }
                                                                case "I2FA":
                                                                    {
                                                                        CustomException.ThrowNew.GenericException("Invalid 2FA code." + Environment.NewLine + message);
                                                                        WindowManager.TwoFactorAuthentication.PromptAgain();
                                                                        break;
                                                                    }
                                                                case "E2FA":
                                                                    {
                                                                        CustomException.ThrowNew.GenericException("Expired 2FA code." + Environment.NewLine + message);
                                                                        break;
                                                                    }
                                                                default:
                                                                    {
                                                                        AutomatedTaskFramework.Tasks.GetCurrentOrDefault()?.Terminate();
                                                                        CustomException.ThrowNew.NetworkException(message, "[ERRNO " + errno + "] " + errID);
                                                                        break;
                                                                    }
                                                            }
                                                            break;
                                                        }
                                                    //HANDLING RETURN VALUES BELOW... IT'S 03:30 AM WTF AM I DOING WITH MY LIFE???
                                                    case "RET":
                                                        {
                                                            switch (decryptedData.Split('!')[1])
                                                            {
                                                                case "LOGIN_SUCCESSFUL":
                                                                    {
                                                                        if (!GlobalVarPool.wasOnline)
                                                                        {
                                                                            new Thread(async delegate ()
                                                                            {
                                                                                await DataBaseHelper.ModifyData(DataBaseHelper.Security.SQLInjectionCheckQuery(new string[] { "UPDATE Tbl_user SET U_wasOnline = 1, U_username = \"", GlobalVarPool.username, "\", U_email = \"",  GlobalVarPool.email,"\";" }));
                                                                                await DataBaseHelper.ModifyData(DataBaseHelper.Security.SQLInjectionCheckQuery(new string[] { "UPDATE Tbl_settings SET S_server_ip = \"", GlobalVarPool.REMOTE_ADDRESS, "\", S_server_port = \"", GlobalVarPool.REMOTE_PORT.ToString(), "\";" }));
                                                                                GlobalVarPool.wasOnline = true;
                                                                            }).Start();
                                                                        }
                                                                        GlobalVarPool.isUser = true;
                                                                        WindowManager.LoadingScreen.InvokeSetStatus("Logged in.");
                                                                        break;
                                                                    }
                                                                case "SEND_VERIFICATION_ACTIVATE_ACCOUNT":
                                                                    {
                                                                        Prompt.Command = PromptCommand.ACTIVATE_ACCOUNT;
                                                                        WindowManager.TwoFactorAuthentication.Prompt("Activate your account", "Please verify your email address.");
                                                                        break;
                                                                    }
                                                                case "LOGGED_OUT":
                                                                    {
                                                                        string previousUser = GlobalVarPool.currentUser;
                                                                        GlobalVarPool.currentUser = "<" + GlobalVarPool.serverName + ">";
                                                                        if (GlobalVarPool.isRoot)
                                                                        {
                                                                            GlobalVarPool.isRoot = false;
                                                                        }
                                                                        else if (GlobalVarPool.isUser)
                                                                        {
                                                                            GlobalVarPool.isUser = false;
                                                                        }
                                                                        break;
                                                                    }
                                                                case "ALREADY_ADMIN":
                                                                    {
                                                                        break;
                                                                    }
                                                                case "SEND_VERIFICATION_ADMIN_NEW_DEVICE":
                                                                    {
                                                                        break;
                                                                    }
                                                                case "SUCCESSFUL_ADMIN_LOGIN":
                                                                    {
                                                                        string previousUser = GlobalVarPool.currentUser;
                                                                        GlobalVarPool.currentUser = "<root@" + GlobalVarPool.serverName + ">";
                                                                        GlobalVarPool.isRoot = true;
                                                                        break;
                                                                    }
                                                                case "COOKIE_DOES_EXIST":
                                                                    {
                                                                        NetworkAdapter.MethodProvider.Authorize();
                                                                        break;
                                                                    }
                                                                case "COOKIE_DOES_NOT_EXIST":
                                                                    {
                                                                        NetworkAdapter.MethodProvider.GetCookie();
                                                                        break;
                                                                    }
                                                                case "ACCOUNT_VERIFIED":
                                                                    {
                                                                        if (!GlobalVarPool.name.Equals("User"))
                                                                        {
                                                                            new Thread(async delegate ()
                                                                            {
                                                                                await DataBaseHelper.ModifyData(DataBaseHelper.Security.SQLInjectionCheckQuery(new string[] { "UPDATE Tbl_user SET U_name = \"", GlobalVarPool.name, "\";" }));
                                                                            }).Start();
                                                                        }
                                                                        break;
                                                                    }
                                                                case "SEND_VERIFICATION_NEW_DEVICE":
                                                                    {
                                                                        Prompt.Command = PromptCommand.CONFIRM_NEW_DEVICE;
                                                                        WindowManager.TwoFactorAuthentication.Prompt("Confirm new device", "Looks like you're trying to login from a new device.");
                                                                        break;
                                                                    }
                                                                case "NOT_LOGGED_IN":
                                                                    {
                                                                        break;
                                                                    }
                                                                case "CODE_RESENT":
                                                                    {
                                                                        CustomException.ThrowNew.NotImplementedException("The code has been resent.");
                                                                        break;
                                                                    }
                                                                case "CLIENT_NOT_FOUND":
                                                                    {
                                                                        break;
                                                                    }
                                                                case "CLIENT_KICKED":
                                                                    {
                                                                        break;
                                                                    }
                                                                case "SEND_VERIFICATION_CHANGE_PASSWORD":
                                                                    {
                                                                        Prompt.Command = PromptCommand.VERIFY_PASSWORD_CHANGE;
                                                                        WindowManager.TwoFactorAuthentication.Prompt("Verify password change", "Looks like your trying to change your password.");
                                                                        break;
                                                                    }
                                                                case "SEND_VERIFICATION_DELETE_ACCOUNT":
                                                                    {
                                                                        break;
                                                                    }
                                                                case "ACCOUNT_DELETED_SUCCESSFULLY":
                                                                    {
                                                                        break;
                                                                    }
                                                                case "PASSWORD_CHANGED":
                                                                    {
                                                                        break;
                                                                    }
                                                                case "SELECT_FINISHED":
                                                                    {
                                                                        break;
                                                                    }
                                                                case "DEVICE_BANNED":
                                                                case "BANNED":
                                                                    {
                                                                        AutomatedTaskFramework.Tasks.GetCurrentOrDefault()?.Terminate();
                                                                        CustomException.ThrowNew.NetworkException("YOU HAVE BEEN BANNED.");
                                                                        break;
                                                                    }
                                                                case "AD_OUTDATED":
                                                                    {
                                                                        new Thread(async delegate ()
                                                                        {
                                                                            string[] details = decryptedData.Split(';');
                                                                            string datetime = null, email = null, name = null;
                                                                            for (int j = 0; j < details.Length; j++)
                                                                            {
                                                                                if (details[j].Contains("datetime"))
                                                                                {
                                                                                    datetime = details[j].Split('!')[1];
                                                                                }
                                                                                else if (details[j].Contains("email"))
                                                                                {
                                                                                    email = details[j].Split('!')[1];
                                                                                }
                                                                                else if (details[j].Contains("name"))
                                                                                {
                                                                                    name = details[j].Split('!')[1];
                                                                                }
                                                                            }
                                                                            if (new string[] { datetime, email, name }.Contains(null))
                                                                            {
                                                                                AutomatedTaskFramework.Tasks.GetCurrentOrDefault()?.Terminate();
                                                                                CustomException.ThrowNew.FormatException("Missing parameters for AD_OUTDATED");
                                                                                return;
                                                                            }
                                                                            await DataBaseHelper.ModifyData(DataBaseHelper.Security.SQLInjectionCheckQuery(new string[] { "UPDATE Tbl_user SET U_name = \"", name, "\", U_email = \"", email, "\", U_datetime = \"", datetime, "\";" }));
                                                                            GlobalVarPool.email = email;
                                                                            GlobalVarPool.name = name;
                                                                            MainForm.InvokeRefreshSettings();
                                                                        }).Start();
                                                                        break;
                                                                    }
                                                                case "AD_UPTODATE":
                                                                    {
                                                                        break;
                                                                    }
                                                                case "NAME_CHANGED":
                                                                    {
                                                                        // TODO: CREATE NOTIFICATION
                                                                        CustomException.ThrowNew.GenericException("Name changed successfully.");
                                                                        break;
                                                                    }
                                                                default:
                                                                    {
                                                                        break;
                                                                    }
                                                            }
                                                            break;
                                                        }
                                                    case "MIR":
                                                        {
                                                            break;
                                                        }
                                                }
                                                break;
                                            }
                                    }
                                    break;
                                }
                            default:
                                {
                                    AutomatedTaskFramework.Tasks.GetCurrentOrDefault()?.Terminate();
                                    CustomException.ThrowNew.NetworkException("Received invalid Packet Specifier.", "[ERRNO 05] IPS");
                                    break;
                                }
                        }
                    }
                }
            }
            catch (SocketException se)
            {
                if (!GlobalVarPool.threadKilled)
                {
                    CustomException.ThrowNew.NetworkException(se.ToString(), "Socket Error:");
                    GlobalVarPool.connectionLost = true;
                }
            }
            catch (Exception e)
            {
                if (!GlobalVarPool.threadKilled)
                {
                    CustomException.ThrowNew.GenericException(e.ToString());
                    GlobalVarPool.connectionLost = true;
                }
            }
            finally
            {
                if (isSocketError)
                {
                    GlobalVarPool.clientSocket.Close();
                    GlobalVarPool.clientSocket.Dispose();
                }
                else if (isTcpFin)
                {
                    // TODO: DISPLAY NON-POP-UP DISCONNECTED MESSAGE
                    try
                    {
                        GlobalVarPool.clientSocket.Disconnect(true);
                    }
                    catch { }
                    GlobalVarPool.clientSocket.Close();
                    GlobalVarPool.clientSocket.Dispose();
                }
                else if (GlobalVarPool.threadKilled)
                {
                    // TODO: DISPLAY NON-POP-UP DISCONNECTED MESSAGE
                }
                else
                {
                    // TODO: DISPLAY NON-POP-UP DISCONNECTED MESSAGE
                    try
                    {
                        if (!isDisconnected)
                        {
                            Network.Send("FIN");
                        }
                        GlobalVarPool.clientSocket.Disconnect(true);
                        GlobalVarPool.clientSocket.Close();
                        GlobalVarPool.clientSocket.Dispose();
                    }
                    catch
                    {
                        GlobalVarPool.clientSocket.Close();
                        GlobalVarPool.clientSocket.Dispose();
                    }
                }
                string previousUser = GlobalVarPool.currentUser;
                GlobalVarPool.currentUser = "<offline>";
                if (GlobalVarPool.isRoot)
                {
                    GlobalVarPool.isRoot = false;
                }
                else if (GlobalVarPool.isUser)
                {
                    GlobalVarPool.isUser = false;
                }
                GlobalVarPool.connected = false;
                GlobalVarPool.ThreadIDs.Remove(Thread.CurrentThread.ManagedThreadId);
                if (GlobalVarPool.ThreadIDs.Count > 0)
                {
                    threadAbort = true;
                }
                threadRunning = false;
                connectionMutex.ReleaseMutex();
            }
        }
    }
}
