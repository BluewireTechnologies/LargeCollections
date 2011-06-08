using System;
using System.Collections.Generic;
using System.Data;

namespace LargeCollections.Storage.Database
{
    public class ObjectDataReader<T> : IDataReader
    {
        private readonly List<IColumnPropertyMapping<T>> columns;
        private readonly IEnumerator<T> source;

        public ObjectDataReader(List<IColumnPropertyMapping<T>> columns, IEnumerator<T> source)
        {
            this.columns = columns;
            this.source = source;
        }

        private bool complete;
        private int count;
        public int GetFinalCount()
        {
            if (!complete) throw new InvalidOperationException("Reader has not yet been completed.");
            return count;
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
                count++;
                return true;
            }
            complete = true;
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
            get { return columns.Count; }
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
            return  typeof(T);
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
            return columns.FindIndex(c => c.Name == name);
        }

        public string GetString(int i)
        {
            throw new NotImplementedException();
        }

        public object GetValue(int i)
        {
            return columns[i].Get(source.Current);
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            return GetValue(i) == null;
        }

        public object this[string name]
        {
            get { return this[GetOrdinal(name)]; }
        }

        public object this[int i]
        {
            get
            {
                return GetValue(i);
            }
        }
    }
}