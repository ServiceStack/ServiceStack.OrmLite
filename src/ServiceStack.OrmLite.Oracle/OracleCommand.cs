using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;

namespace ServiceStack.OrmLite.Oracle
{
    public class OracleCommand : DbCommand
    {
        private readonly DbCommand _command;
        public OracleCommand(DbCommand command)
        {
            _command = command;
        }

        public override void Prepare()
        {
            _command.Prepare();
        }

        public override string CommandText
        {
            get { return _command.CommandText; }
            set { _command.CommandText = value; }
        }

        public override int CommandTimeout
        {
            get { return _command.CommandTimeout; }
            set { _command.CommandTimeout = value; }
        }

        public override CommandType CommandType
        {
            get { return _command.CommandType; }
            set { _command.CommandType = value; }
        }

        protected override DbConnection DbConnection
        {
            get { return _command.Connection; }
            set
            {
                _command.Connection = value;
            }
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return _command.Parameters; }
        }

        protected override DbTransaction DbTransaction
        {
            get { return _command.Transaction; }
            set { _command.Transaction = value; }
        }

        public override bool DesignTimeVisible
        {
            get { return _command.DesignTimeVisible; }
            set { _command.DesignTimeVisible = value; }
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get { return _command.UpdatedRowSource; }
            set { _command.UpdatedRowSource = value; }
        }

        public override void Cancel()
        {
            _command.Cancel();
        }

        protected override DbParameter CreateDbParameter()
        {
            return _command.CreateParameter();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            ReorderParameters();
            var reader = _command.ExecuteReader(behavior);
            return new OracleDataReader(reader);
        }

        private void ReorderParameters()
        {
            var parameters = new Dictionary<int, IDbDataParameter>();

            foreach (DbParameter parameter in Parameters)
            {
                var parameterName = GetParameterName(parameter);
                var indexes = GetParameterIndexes(CommandText, parameterName);

                foreach (var index in indexes)
                {
                    if (parameters.ContainsKey(index)) continue;
                    
                    //var p = _command.CreateParam(parameter.ParameterName.TrimStart(':'), parameter.Value, parameter.Direction, parameter.DbType);
                    parameters.Add(index, parameter);
                }
            }

            Parameters.Clear();

            foreach (var item in parameters.OrderBy(p => p.Key))
            {
                _command.Parameters.Add(item.Value);
            }
        }

        private IEnumerable<int> GetParameterIndexes(string commandText, string parameterName)
        {
            var regex = new Regex(parameterName + "[\\W?]", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var matches = regex.Matches(string.Format("{0} ", commandText));
            return from Match m in matches select m.Index;
        }

        private string GetParameterName(DbParameter parameter)
        {
            return parameter.ParameterName.StartsWith(":")
                ? parameter.ParameterName
                : string.Format(":{0}", parameter.ParameterName);
        }

        public override int ExecuteNonQuery()
        {
            ReorderParameters();
            return _command.ExecuteNonQuery();
        }

        public override object ExecuteScalar()
        {
            ReorderParameters();
            return _command.ExecuteScalar();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _command.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
