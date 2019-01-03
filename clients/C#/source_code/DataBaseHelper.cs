﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pmdbs
{
    class DataBaseHelper
    {
        private static SQLiteConnection sql_con;
        private static SQLiteCommand sql_cmd;

        private static void SetConnection()
        {
            string dataSource = "Resources\\localdata_windows.db";

            sql_con = new SQLiteConnection
            {
                ConnectionString = "Data Source=" + dataSource
            };
            sql_con.Open();
        }
        /// <summary>
        /// Returns result of SQLite database query as List<String> Object.
        /// </summary>
        /// <param name="query">SQLite query to be executed.</param>
        /// <param name="columns">Number of by query returned columns.</param>
        /// <returns></returns
        public static async Task<List<String>> GetDataAsList(string query, int columns)
        {
            SetConnection();
            sql_cmd = sql_con.CreateCommand();
            sql_cmd.CommandText = query;
            await sql_cmd.ExecuteNonQueryAsync();
            SQLiteDataReader reader = sql_cmd.ExecuteReader();
            List<String> DataList = new List<string>();
            while (reader.Read())
            {
                try
                {
                    for (int i = 0; i < columns; i++)
                    {
                        DataList.Add(reader[i].ToString());
                    }
                }
                catch (Exception  outOfRange)
                {
                    Console.WriteLine("Error 'Reader out of range' occured: '{0}'", outOfRange);
                }
            }
            sql_con.Close();
            sql_con.Dispose();
            return DataList;
        }
        /// <summary>
        /// Returns result of SQLite database query as List<List<String>> Object.
        /// </summary>
        /// <param name="query">SQLite query to be executed.</param>
        /// <param name="columns">Number of by query returned columns.</param>
        /// <returns></returns>
        public static async Task<List<List<String>>> GetDataAs2DList(string query, int columns)
        {
            SetConnection();
            sql_cmd = sql_con.CreateCommand();
            sql_cmd.CommandText = query;
            await sql_cmd.ExecuteNonQueryAsync();
            SQLiteDataReader reader = sql_cmd.ExecuteReader();
            List<List<String>> OuterList = new List<List<string>>();
            while (reader.Read())
            {
                try
                {
                    List<String> InnerList = new List<string>();
                    for (int i = 0; i < columns; i++)
                    {
                        InnerList.Add(reader[i].ToString());
                    }
                }
                catch (Exception outOfRange)
                {
                    Console.WriteLine("Error 'Reader out of range' occured: '{0}'", outOfRange);
                }
            }
            sql_con.Close();
            sql_con.Dispose();
            return OuterList;
        }
        /// <summary>
        /// Returns result of SQLite database query as DataTable Object.
        /// </summary>
        /// <param name="query">SQLite query to be executed.</param>
        /// <param name="columns">Number of by query returned columns.</param>
        /// <returns></returns>
        public static async Task<DataTable> GetDataAsDataTable(string query, int columns)
        {
            SetConnection();
            sql_cmd = sql_con.CreateCommand();
            sql_cmd.CommandText = query;
            await sql_cmd.ExecuteNonQueryAsync();
            SQLiteDataReader reader = sql_cmd.ExecuteReader();
            DataTable ReturnData = new DataTable();
            for (int i = 0; i < columns; i++)
            {
                ReturnData.Columns.Add(i.ToString(), typeof(string));
            }
            while (reader.Read())
            {
                try
                {
                    DataRow NewRow = ReturnData.Rows.Add();
                    for (int i = 0; i < columns; i++)
                    {
                        NewRow[i.ToString()] = reader[i].ToString();
                    }
                }
                catch (Exception outOfRange)
                {
                    Console.WriteLine("Error 'Reader out of range' occured: '{0}'", outOfRange);
                }
            }
            sql_con.Close();
            sql_con.Dispose();
            return ReturnData;
        }
        /// <summary>
        /// Executes a SQLite query to manipulate data.
        /// </summary>
        /// <param name="query">SQLite query to be executed.</param>
        /// <returns></returns>
        public static async Task ModifyData(string query)
        {
            SetConnection();
            sql_cmd = sql_con.CreateCommand();
            sql_cmd.CommandText = query;
            await sql_cmd.ExecuteNonQueryAsync();
            sql_con.Close();
            sql_con.Dispose();
        }
    }
}