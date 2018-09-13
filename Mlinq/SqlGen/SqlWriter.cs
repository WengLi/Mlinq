using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Mlinq.SqlGen
{
    internal class SqlWriter
    {
        private StringBuilder _sb;
        public const string DefaultTabString = "    ";
        public static readonly CultureInfo Culture = CultureInfo.InvariantCulture;
        private int _indentLevel;
        private bool _tabsPending;
        private readonly string _tabString;
        private readonly List<string> _cachedIndents = new List<string>();
        private string _newLine = Environment.NewLine;

        public Encoding Encoding
        {
            get { return new UnicodeEncoding(false, false); }
        }

        public int Indent
        {
            get { return _indentLevel; }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                _indentLevel = value;
            }
        }

        public string NewLine
        {
            get { return _newLine; }
            set { _newLine = value; }
        }

        public SqlWriter(StringBuilder sb)
        {
            _sb = sb;
        }

        protected virtual void OutputTabs()
        {
            if (!_tabsPending)
            {
                return;
            }
            _sb.Append(CurrentIndentation());
            _tabsPending = false;
        }

        public virtual string CurrentIndentation()
        {
            if (_indentLevel <= 0 || String.IsNullOrEmpty(_tabString))
            {
                return String.Empty;
            }

            if (_indentLevel == 1)
            {
                return _tabString;
            }

            var cacheIndex = _indentLevel - 2;
            var cached = cacheIndex < _cachedIndents.Count ? _cachedIndents[cacheIndex] : null;

            if (cached == null)
            {
                cached = BuildIndent(_indentLevel);

                if (cacheIndex == _cachedIndents.Count)
                {
                    _cachedIndents.Add(cached);
                }
                else
                {
                    for (var i = _cachedIndents.Count; i <= cacheIndex; i++)
                    {
                        _cachedIndents.Add(null);
                    }
                    _cachedIndents[cacheIndex] = cached;
                }
            }
            return cached;
        }

        private string BuildIndent(int numberOfIndents)
        {
            var sb = new StringBuilder(numberOfIndents * _tabString.Length);

            for (var index = 0; index < numberOfIndents; ++index)
            {
                sb.Append(_tabString);
            }
            return sb.ToString();
        }

        public void Write(string value)
        {
            OutputTabs();
            _sb.Append(value);

            if (value != null && (value.Equals("\r\n", StringComparison.Ordinal) || value.Equals("\n", StringComparison.Ordinal)))
            {
                _tabsPending = true;
            }
        }

        public virtual void Write(uint value)
        {
            Write(value.ToString(Culture));
        }

        public void Write(bool value)
        {
            OutputTabs();
            Write(value ? "True" : "False");
        }

        public void Write(char value)
        {
            OutputTabs();
            _sb.Append(value);
        }

        public void Write(char[] buffer)
        {
            OutputTabs();
            Write(buffer, 0, buffer.Length);
        }

        public void Write(char[] buffer, int index, int count)
        {
            OutputTabs();
            _sb.Append(buffer, index, count);
        }

        public void Write(double value)
        {
            OutputTabs();
            Write(value.ToString(Culture));
        }

        public void Write(float value)
        {
            OutputTabs();
            Write(value.ToString(Culture));
        }

        public void Write(int value)
        {
            OutputTabs();
            Write(value.ToString(Culture));
        }

        public void Write(long value)
        {
            OutputTabs();
            Write(value.ToString(Culture));
        }

        public void Write(object value)
        {
            OutputTabs();
            if (value != null)
            {
                IFormattable formattable = value as IFormattable;
                if (formattable != null)
                {
                    Write(formattable.ToString(null, Culture));
                }
                else
                {
                    Write(value.ToString());
                }
            }
        }

        public void Write(string format, object arg0)
        {
            OutputTabs();
            Write(string.Format(Culture, format, arg0));
        }

        public void Write(string format, object arg0, object arg1)
        {
            OutputTabs();
            Write(string.Format(Culture, format, arg0, arg1));
        }

        public void Write(string format, params object[] arg)
        {
            OutputTabs();
            Write(string.Format(Culture, format, arg));
        }

        public void WriteLineNoTabs(string value)
        {
            WriteLine(value);
        }

        public void WriteLine(string value)
        {
            OutputTabs();
            if (value == null)
            {
                WriteLine();
            }
            else
            {
                int length = value.Length;
                int num = NewLine.Length;
                char[] array = new char[length + num];
                value.CopyTo(0, array, 0, length);
                switch (num)
                {
                    case 2:
                        array[length] = NewLine[0];
                        array[length + 1] = NewLine[1];
                        break;
                    case 1:
                        array[length] = NewLine[0];
                        break;
                    default:
                        Buffer.BlockCopy(NewLine.ToCharArray(), 0, array, length * 2, num * 2);
                        break;
                }
                Write(array, 0, length + num);
            }
            _tabsPending = true;
        }

        public void WriteLine()
        {
            OutputTabs();
            Write(NewLine);
            _tabsPending = true;
        }

        public void WriteLine(bool value)
        {
            OutputTabs();
            Write(value);
            WriteLine();
            _tabsPending = true;
        }

        public void WriteLine(char value)
        {
            OutputTabs();
            Write(value);
            WriteLine();
            _tabsPending = true;
        }

        public void WriteLine(char[] buffer)
        {
            OutputTabs();
            Write(buffer);
            WriteLine();
            _tabsPending = true;
        }

        public void WriteLine(char[] buffer, int index, int count)
        {
            OutputTabs();
            Write(buffer, index, count);
            WriteLine();
            _tabsPending = true;
        }

        public void WriteLine(double value)
        {
            OutputTabs();
            Write(value);
            WriteLine();
            _tabsPending = true;
        }

        public void WriteLine(float value)
        {
            OutputTabs();
            Write(value);
            WriteLine();
            _tabsPending = true;
        }

        public void WriteLine(int value)
        {
            OutputTabs();
            Write(value);
            WriteLine();
            _tabsPending = true;
        }

        public void WriteLine(long value)
        {
            OutputTabs();
            Write(value);
            WriteLine();
            _tabsPending = true;
        }

        public void WriteLine(object value)
        {
            OutputTabs();
            Write(value);
            WriteLine();
            _tabsPending = true;
        }

        public void WriteLine(string format, object arg0)
        {
            OutputTabs();
            WriteLine(string.Format(Culture, format, arg0));
            _tabsPending = true;
        }

        public void WriteLine(string format, object arg0, object arg1)
        {
            OutputTabs();
            WriteLine(string.Format(Culture, format, arg0, arg1));
            _tabsPending = true;
        }

        public void WriteLine(string format, params object[] arg)
        {
            OutputTabs();
            WriteLine(string.Format(Culture, format, arg));
            _tabsPending = true;
        }

        public void WriteLine(uint value)
        {
            OutputTabs();
            Write(value);
            WriteLine();
            _tabsPending = true;
        }
    }
}
