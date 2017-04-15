using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace UnityPatch
{
    public class Patcher : IDisposable
    {
        private int _bufferSize = 2048;
        private FileStream _fs;
        private bool _isDisposed;

        private string[] _succesMessages = new string[5]
        {
            "Pattern not found!!",
            "Patched!!",
            "Patched but result is other!!",
            "Found!!",
            "Found but result is other!!"
        };

        public string FileName;

        public Patcher()
        {
            Patterns = new List<Pattern>();
        }

        public Patcher(string filename)
        {
            Patterns = new List<Pattern>();
            FileName = filename;
        }

        public Patcher(string filename, List<Pattern> patterns)
        {
            if (Patterns == null)
                throw new ArgumentNullException("patterns.");
            if (Patterns.Count == 0)
                throw new ArgumentOutOfRangeException("Collection is empty: patterns.");
            Patterns = patterns;
            FileName = filename;
        }

        public PatcherState CurrentState { get; private set; }

        public string[] SuccesMessages
        {
            get => _succesMessages;
            set
            {
                if (value == null || value.Length != 5)
                    return;
                _succesMessages = value;
            }
        }

        public int BufferSize
        {
            get => _bufferSize;
            set => _bufferSize = value < 16 ? 16 : value;
        }

        public List<Pattern> Patterns { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Patcher()
        {
            Dispose(false);
        }

        public bool AddString(string se, string rep, uint brakAfter = 1, uint replaceAfter = 0)
        {
            try
            {
                Patterns.Add(new Pattern(se, rep, brakAfter, replaceAfter));
            }
            catch (Exception ex)
            {
                NLogger.Warn(string.Format("AddString: {0}: {1}", ex.GetType(), ex.Message));
                return false;
            }
            return true;
        }

        public bool Patch()
        {
            if (Patterns == null)
            {
                NLogger.Error("ArgumentNullException: patterns.");
                return false;
            }
            Patterns.RemoveAll(item => item == null);
            if (Patterns.Count == 0)
            {
                NLogger.Error("Collection is empty: patterns.");
                return false;
            }
            uint fail = 0;
            uint good = 0;
            var fileHasChanged = false;
            try
            {
                _fs = CreateStream(FileName);
                foreach (var pattern in Patterns)
                {
                    var allPatterns = FindAllPatterns(pattern);
                    if (allPatterns.SuccessfullyFound > 0)
                    {
                        if (ReplaceAllPatterns(allPatterns))
                            fileHasChanged = true;
                        if (allPatterns.Success)
                        {
                            ++good;
                        }
                        else
                        {
                            ++good;
                            ++fail;
                        }
                    }
                    else
                    {
                        ++fail;
                    }
                }
            }
            catch (Exception ex)
            {
                if (_fs != null)
                {
                    _fs.Dispose();
                    _fs = null;
                }
                NLogger.Error(string.Format("{0}: {1}", ex.GetType(), ex.Message));
                return false;
            }
            if (_fs != null)
            {
                _fs.Close();
                _fs = null;
            }
            if (fileHasChanged)
            {
                if (fail > 0U && good > 0U)
                {
                    NLogger.Debug(_succesMessages[2]);
                    CurrentState = new PatcherState(fileHasChanged, good, fail, _succesMessages[2]);
                    return true;
                }
                if ((int) fail == 0 && good > 0U)
                {
                    NLogger.Debug(_succesMessages[1]);
                    CurrentState = new PatcherState(fileHasChanged, good, fail, _succesMessages[1]);
                    return true;
                }
                CurrentState = new PatcherState(fileHasChanged, good, fail, _succesMessages[0]);
                NLogger.Debug(_succesMessages[0]);
            }
            else
            {
                if (fail > 0U && good > 0U)
                {
                    CurrentState = new PatcherState(fileHasChanged, good, fail, _succesMessages[4]);
                    NLogger.Debug(_succesMessages[4]);
                    return true;
                }
                if ((int) fail == 0 && good > 0U)
                {
                    CurrentState = new PatcherState(fileHasChanged, good, fail, _succesMessages[3]);
                    NLogger.Debug(_succesMessages[3]);
                    return true;
                }
                CurrentState = new PatcherState(fileHasChanged, good, fail, _succesMessages[0]);
                NLogger.Debug(_succesMessages[0]);
            }
            return false;
        }

        private MainPattern FindAllPatterns(Pattern pt)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("Object was disposed.");
            if (pt == null)
                throw new ArgumentNullException("Pattern");
            if (!pt.ValidPattern)
                return new MainPattern(pt, null);
            long num1 = 0;
            var num2 = 0;
            var index1 = 0;
            uint num3 = 0;
            uint num4 = 0;
            var buffer = new byte[_bufferSize];
            var num5 = _bufferSize;
            var searchBytes = pt.SearchBytes;
            var longList = new List<long>();
            if (_isDisposed)
                throw new ObjectDisposedException("Object was disposed.");
            _fs.Position = 0L;
            while (num5 > 0)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException("Object was disposed.");
                var position = _fs.Position;
                num5 = _fs.Read(buffer, 0, buffer.Length);
                for (var index2 = 0; index2 < num5; ++index2)
                    if (searchBytes[index1].Act == Pattern.UniversalByte.Action.Skip ||
                        buffer[index2] == searchBytes[index1].B)
                    {
                        if (index1 == 0)
                        {
                            num1 = position;
                            num2 = index2;
                        }
                        if (searchBytes.Length - 1 == index1)
                        {
                            index1 = 0;
                            if ((int) num3 == (int) pt.ReplaceAfter)
                            {
                                ++num4;
                                longList.Add(num1 + num2);
                                if (pt.BrakAfter > 0U && (int) num4 == (int) pt.BrakAfter)
                                    return new MainPattern(pt, longList.ToArray());
                            }
                            else
                            {
                                ++num3;
                            }
                        }
                        else
                        {
                            ++index1;
                        }
                    }
                    else if (index1 > 0)
                    {
                        index1 = 0;
                        index2 = num2;
                        if (num1 != position)
                        {
                            _fs.Position = num1;
                            num5 = _fs.Read(buffer, 0, buffer.Length);
                        }
                    }
            }
            if (num4 <= 0U)
                return new MainPattern(pt, null);
            return new MainPattern(pt, longList.ToArray());
        }

        private bool ReplaceAllPatterns(MainPattern pt)
        {
            if (!pt.Ptrn.ReplaceImmediately || _isDisposed)
                return false;
            var num1 = 0;
            var streamPosition = pt.StreamPosition;
            var replaceBytes = pt.Ptrn.ReplaceBytes;
            var buffer = new byte[pt.Ptrn.SearchBytes.Length];
            foreach (var num2 in streamPosition)
            {
                if (_isDisposed)
                    return false;
                var flag = false;
                _fs.Position = num2;
                _fs.Read(buffer, 0, buffer.Length);
                for (var index = 0; index < buffer.Length; ++index)
                    if (replaceBytes[index].Act > Pattern.UniversalByte.Action.Skip &&
                        buffer[index] != replaceBytes[index].B)
                    {
                        buffer[index] = replaceBytes[index].B;
                        flag = true;
                    }
                if (flag)
                {
                    _fs.Position = num2;
                    _fs.Write(buffer, 0, buffer.Length);
                    ++num1;
                }
            }
            return true;
        }

        private FileStream CreateStream(string fileName)
        {
            File.GetAttributes(fileName);
            var fileAttributes = FileAttributes.Normal;
            File.SetAttributes(fileName, fileAttributes);
            var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite);
            if (fileStream.Length < 16L)
            {
                fileStream.Close();
                throw new IOException("File is too short! (posible damaged).");
            }
            return fileStream;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;
            if (disposing && _fs != null)
                _fs.Dispose();
            _fs = null;
            _isDisposed = true;
        }

        public class PatcherState
        {
            public readonly uint Fail;
            public readonly bool FileHasChanged;
            public readonly uint Good;
            public readonly string StringState;

            public PatcherState(bool fileHasChanged, uint good, uint fail, string stringState = "")
            {
                FileHasChanged = fileHasChanged;
                Good = good;
                Fail = fail;
                StringState = stringState;
            }
        }

        public struct MainPattern
        {
            public readonly Pattern Ptrn;
            public readonly long[] StreamPosition;

            public int SuccessfullyFound
            {
                get
                {
                    if (StreamPosition == null)
                        return 0;
                    return StreamPosition.Length;
                }
            }

            public bool Success
            {
                get
                {
                    if (StreamPosition == null)
                        return false;
                    if (Ptrn.BrakAfter < 1U && StreamPosition.Length != 0)
                        return true;
                    return Ptrn.BrakAfter - Ptrn.ReplaceAfter == StreamPosition.Length;
                }
            }

            public MainPattern(Pattern ptrn, long[] streamPosition)
            {
                if (ptrn == null)
                    throw new ArgumentNullException("Pattern.");
                Ptrn = ptrn;
                StreamPosition = streamPosition;
            }
        }

        public class Pattern
        {
            private readonly bool _valid;
            public readonly UniversalByte[] ReplaceBytes;
            public readonly UniversalByte[] SearchBytes;
            public uint BrakAfter;
            public uint ReplaceAfter;

            public Pattern(string se, string rep, uint brakAfter = 1, uint replaceAfter = 0)
            {
                if (string.IsNullOrEmpty(se))
                    throw new ArgumentException("Input string null or empty.");
                BrakAfter = brakAfter;
                ReplaceAfter = replaceAfter;
                if (rep == null)
                {
                    SearchBytes = TryParse(se);
                    ReplaceImmediately = false;
                    _valid = true;
                }
                else
                {
                    SearchBytes = TryParse(se);
                    ReplaceBytes = TryParse(rep);
                    ReplaceImmediately = true;
                    _valid = true;
                }
            }

            public bool ValidPattern
            {
                get
                {
                    if (BrakAfter > 0U && BrakAfter - ReplaceAfter < 1U)
                        return false;
                    return _valid;
                }
            }

            public bool ReplaceImmediately { get; }

            private UniversalByte[] TryParse(string st)
            {
                var stringBuilder = new StringBuilder();
                foreach (var ch in st)
                    if (ch > 32 && ch < sbyte.MaxValue)
                        stringBuilder.Append(ch);
                var upper = stringBuilder.ToString().ToUpper();
                if (upper.Length % 2 != 0)
                    throw new Exception("String of byte must be power of two.");
                var flag = false;
                var universalByteArray = new UniversalByte[upper.Length / 2];
                var index = 0;
                var startIndex = 0;
                while (startIndex < upper.Length)
                {
                    var s = upper.Substring(startIndex, 2);
                    if (s == "??")
                    {
                        universalByteArray[index].Act = UniversalByte.Action.Skip;
                    }
                    else
                    {
                        universalByteArray[index].Act = UniversalByte.Action.Normal;
                        universalByteArray[index].B = byte.Parse(s, NumberStyles.HexNumber);
                        flag = true;
                    }
                    ++index;
                    startIndex += 2;
                }
                if (!flag)
                    throw new ArgumentNullException("Can't add the string of byte (Does not make sense).");
                return universalByteArray;
            }

            public struct UniversalByte
            {
                public Action Act;
                public byte B;

                public enum Action
                {
                    Skip,
                    Normal
                }
            }
        }
    }
}