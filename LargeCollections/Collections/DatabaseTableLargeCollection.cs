using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using LargeCollections.Resources;

namespace LargeCollections.Collections
{
    public class DatabaseTableLargeCollection<T> : LargeCollectionWithBackingStore<T, DatabaseTableReference>
    {
        public DatabaseTableLargeCollection(DatabaseTableReference reference, long itemCount) : base(reference, itemCount)
        {
        }

        protected override IEnumerator<T> GetEnumeratorImplementation()
        {
            using (var command = BackingStore.Connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = String.Format("SELECT [value] FROM [{0}]", BackingStore.TableName);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return (T)reader[0];
                    }
                }
            }
        }

    }

    class BulkInsertableEnumerator<T> : IDataReader
    {
        private readonly IEnumerator<T> source;
        private readonly SqlDbType sqlDbType;
        private readonly Action increment;

        public BulkInsertableEnumerator(IEnumerator<T> source, SqlDbType sqlDbType, Action increment)
        {
            this.source = source;
            this.sqlDbType = sqlDbType;
            this.increment = increment;
        }

        public void Close()
        {
            source.Dispose();
            IsClosed = true;
        }

        public int Depth
        {
            get { throw new NotImplementedException(); }
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public bool IsClosed { get; private set; }

        public bool NextResult()
        {
            return false;
        }

        public bool Read()
        {
            if (source.MoveNext())
            {
                increment();
                return true;
            }
            return false;
        }

        public int RecordsAffected
        {
            get { throw new NotImplementedException(); }
        }

        public void Dispose()
        {
            Close();
        }

        public int FieldCount
        {
            get { return 1; }
        }

        public bool GetBoolean(int i)
        {
            throw new NotImplementedException();
        }

        public byte GetByte(int i)
        {
            throw new NotImplementedException();
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            throw new NotImplementedException();
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            throw new NotImplementedException();
        }

        public decimal GetDecimal(int i)
        {
            throw new NotImplementedException();
        }

        public double GetDouble(int i)
        {
            throw new NotImplementedException();
        }

        public Type GetFieldType(int i)
        {
            return typeof(T);
        }

        public float GetFloat(int i)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i)
        {
            throw new NotImplementedException();
        }

        public int GetInt32(int i)
        {
            throw new NotImplementedException();
        }

        public long GetInt64(int i)
        {
            throw new NotImplementedException();
        }

        public string GetName(int i)
        {
            throw new NotImplementedException();
        }

        public int GetOrdinal(string name)
        {
            if (name == "value") return 0;
            return -1;
        }

        public string GetString(int i)
        {
            throw new NotImplementedException();
        }

        public object GetValue(int i)
        {
            return source.Current;
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            return false;
        }

        public object this[string name]
        {
            get { return this[GetOrdinal(name)]; }
        }

        public object this[int i]
        {
            get
            {
                if (i == 0) return source.Current;
                throw new IndexOutOfRangeException();
            }
        }
    }

}
