using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Threading.Tasks;
using Avalonia.Threading;
using MsBox.Avalonia;

namespace D8_Demo.Tool;

public class Mdb
{
    //批量执行sql
    public static async Task ExecuteBatch(string filePath, List<string> sqls)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var _connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};";
            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                var command = new OleDbCommand { Connection = connection, Transaction = transaction };

                try
                {
                    foreach (var sql in sqls)
                    {
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        });
    }
    //执行查找并将数据返回
    public static  List<string> Select(string filePath, string sql)
    {
        var _connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};";

        using (var connection = new OleDbConnection(_connectionString))
        {
            try
            {
                connection.Open();
                var command = new OleDbCommand(sql, connection);
                var reader = command.ExecuteReader();

                if (reader.FieldCount == 0) return null;
                //将结果封装到List<string>集合中
                List<string> result = new List<string>();
                while (reader.Read())
                    for (var i = 0; i < reader.FieldCount; i++)
                        result.Add(reader[i].ToString() ?? throw new InvalidOperationException());

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }

    //执行sql语句不返回数据，可以支持增删改
    public static async Task<bool> Execute(string filePath, string sql)
    {
        var _connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};";

        using (var connection = new OleDbConnection(_connectionString))
        {
            try
            {
                connection.Open();
                var command = new OleDbCommand(sql, connection);
                var result = command.ExecuteNonQuery();
                if (result == 0) return false;
            }
            catch (Exception ex)
            {
                await MessageBoxManager.GetMessageBoxStandard("异常", ex.Message).ShowAsync();
                throw new Exception(ex.Message);
            }
        }
        return true;
    }
  
    
}