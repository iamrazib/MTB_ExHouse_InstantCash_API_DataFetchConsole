﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICTCDataAutoFetchConsole
{
    public class SQLDBManager : IDBManager
    {
        #region Private member variables

        private string _connectionString;
        private string _dataSource;
        private string _initialCatalog;
        private string _userName;
        private string _password;
        private SqlConnection _sqlConnection;
        private SqlCommand _sqlCommand;
        private SqlTransaction _sqlTransaction;

        #endregion

        #region Transaction
        public void BeginTransaction()
        {
            _sqlTransaction = _sqlConnection.BeginTransaction(IsolationLevel.ReadCommitted);
        }
        public void Commit()
        {
            _sqlTransaction.Commit();
        }

        public void RollBack()
        {
            _sqlTransaction.Rollback();
        }
        #endregion

        #region IDatabaseManager Members

        public string SetConnectionString
        {
            set { _connectionString = value; }
        }

        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        public string DatabaseName
        {
            get { return _initialCatalog; }
            set { _initialCatalog = value; }
        }

        public string ServerName
        {
            get { return _dataSource; }
            set { _dataSource = value; }
        }

        public string Password
        {
            set { _password = value; }
        }

        public void BuildConnectionString()
        {
            _connectionString =
                String.Format(@"Data Source={0};Initial Catalog={1};User Id = {2}; Password ={3};", _dataSource, _initialCatalog, _userName, _password);
        }

        public void OpenDatabaseConnection()
        {
            if (_connectionString == string.Empty)
            {
                return;
            }

            try
            {
                if (_sqlConnection == null)
                {
                    _sqlConnection = new SqlConnection(_connectionString);
                }

                if (_sqlConnection.State != ConnectionState.Open)
                {
                    _sqlConnection.Open();
                }
            }
            catch (InvalidOperationException invalidOperationException)
            {
                throw invalidOperationException;
            }
            catch (SqlException sqlException)
            {
                throw sqlException;
            }
        }

        public void CloseDatabaseConnection()
        {
            try
            {
                if (_sqlConnection != null && _sqlConnection.State == ConnectionState.Open)
                {
                    _sqlConnection.Close();
                }
            }
            catch (SqlException sqlException)
            {
                throw sqlException;
            }
        }

        public DataTable GetDataTable(string query)
        {
            DataTable aDataTable = new DataTable();

            if (_sqlConnection == null || _sqlConnection.State != ConnectionState.Open)
            {
                return aDataTable;
            }

            SqlDataAdapter aSqlDataAdapter;
            try
            {
                _sqlCommand = _sqlConnection.CreateCommand();
                _sqlCommand.CommandTimeout = 1000000000;
                _sqlCommand.CommandText = query;
                aSqlDataAdapter = new SqlDataAdapter(_sqlCommand);
                aSqlDataAdapter.Fill(aDataTable);
                return aDataTable;
            }
            catch (InvalidOperationException invalidOperationException)
            {
                throw invalidOperationException;
            }
        }

        public string GetSingleString(string query)
        {
            DataTable dataTable = new DataTable();
            string data = String.Empty;
            if (_sqlConnection == null || _sqlConnection.State != ConnectionState.Open)
            {
                return null;
            }

            try
            {
                _sqlCommand = _sqlConnection.CreateCommand();
                _sqlCommand.CommandText = query;
                _sqlCommand.CommandType = CommandType.Text;
                return _sqlCommand.ExecuteScalar().ToString();

            }
            catch (InvalidOperationException invalidOperationException)
            {
                throw invalidOperationException;
            }
        }

        public bool ExcecuteCommand(string commandText)
        {
            if (_sqlConnection == null || _sqlConnection.State != ConnectionState.Open)
            {
                return false;
            }

            try
            {
                _sqlCommand = _sqlConnection.CreateCommand();
                _sqlCommand.CommandText = commandText;
                _sqlCommand.CommandType = CommandType.Text;
                _sqlCommand.Transaction = _sqlTransaction;
                if (_sqlCommand.ExecuteNonQuery() > 0)
                    return true;
            }
            catch (InvalidOperationException invalidOperationException)
            {
                throw invalidOperationException;
            }
            return false;
        }

        public string GetResponse()
        {
            return "Hi From Sql DataBase Manager";
        }

        #endregion
    }
}
