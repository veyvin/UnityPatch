using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPatch
{
    public class Patcher : IDisposable
    {
        private int _bufferSize = 2048;
        private string[] _succesMessages = new string[5]
        {
      "Pattern not found!!",
      "Patched!!",
      "Patched but result is other!!",
      "Found!!",
      "Found but result is other!!"
        };
        public string FileName;
        private FileStream _fs;
        private bool _isDisposed;

        public Patcher.PatcherState CurrentState { get; private set; }

        public string[] SuccesMessages
        {
            get
            {
                return this._succesMessages;
            }
            set
            {
                if (value == null || value.Length != 5)
                    return;
                this._succesMessages = value;
            }
        }

        public int BufferSize
        {
            get
            {
                return this._bufferSize;
            }
            set
            {
                this._bufferSize = value < 16 ? 16 : value;
            }
        }

        public List<Patcher.Pattern> Patterns { get; }

        public Patcher()
        {
            this.Patterns = new List<Patcher.Pattern>();
        }

        public Patcher(string filename)
        {
            this.Patterns = new List<Patcher.Pattern>();
            this.FileName = filename;
        }

        public Patcher(string filename, List<Patcher.Pattern> patterns)
        {
            if (this.Patterns == null)
                throw new ArgumentNullException("patterns.");
            if (this.Patterns.Count == 0)
                throw new ArgumentOutOfRangeException("Collection is empty: patterns.");
            this.Patterns = patterns;
            this.FileName = filename;
        }

        ~Patcher()
        {
            this.Dispose(false);
        }

        public bool AddString(string se, string rep, uint brakAfter = 1, uint replaceAfter = 0)
        {
            try
            {
                this.Patterns.Add(new Patcher.Pattern(se, rep, brakAfter, replaceAfter));
            }
            catch (Exception ex)
            {
                NLogger.Warn(string.Format("AddString: {0}: {1}", (object)ex.GetType(), (object)ex.Message));
                return false;
            }
            return true;
        }

        public bool Patch()
        {
            if (this.Patterns == null)
            {
                NLogger.Error("ArgumentNullException: patterns.");
                return false;
            }
            this.Patterns.RemoveAll((Predicate<Patcher.Pattern>)(item => item == null));
            if (this.Patterns.Count == 0)
            {
                NLogger.Error("Collection is empty: patterns.");
                return false;
            }
            uint fail = 0;
            uint good = 0;
            bool fileHasChanged = false;
            try
            {
                this._fs = this.CreateStream(this.FileName);
                foreach (Patcher.Pattern pattern in this.Patterns)
                {
                    Patcher.MainPattern allPatterns = this.FindAllPatterns(pattern);
                    if (allPatterns.SuccessfullyFound > 0)
                    {
                        if (this.ReplaceAllPatterns(allPatterns))
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
                        ++fail;
                }
            }
            catch (Exception ex)
            {
                if (this._fs != null)
                {
                    this._fs.Dispose();
                    this._fs = (FileStream)null;
                }
                NLogger.Error(string.Format("{0}: {1}", (object)ex.GetType(), (object)ex.Message));
                return false;
            }
            if (this._fs != null)
            {
                this._fs.Close();
                this._fs = (FileStream)null;
            }
            if (fileHasChanged)
            {
                if (fail > 0U && good > 0U)
                {
                    NLogger.Debug(this._succesMessages[2]);
                    this.CurrentState = new Patcher.PatcherState(fileHasChanged, good, fail, this._succesMessages[2]);
                    return true;
                }
                if ((int)fail == 0 && good > 0U)
                {
                    NLogger.Debug(this._succesMessages[1]);
                    this.CurrentState = new Patcher.PatcherState(fileHasChanged, good, fail, this._succesMessages[1]);
                    return true;
                }
                this.CurrentState = new Patcher.PatcherState(fileHasChanged, good, fail, this._succesMessages[0]);
                NLogger.Debug(this._succesMessages[0]);
            }
            else
            {
                if (fail > 0U && good > 0U)
                {
                    this.CurrentState = new Patcher.PatcherState(fileHasChanged, good, fail, this._succesMessages[4]);
                    NLogger.Debug(this._succesMessages[4]);
                    return true;
                }
                if ((int)fail == 0 && good > 0U)
                {
                    this.CurrentState = new Patcher.PatcherState(fileHasChanged, good, fail, this._succesMessages[3]);
                    NLogger.Debug(this._succesMessages[3]);
                    return true;
                }
                this.CurrentState = new Patcher.PatcherState(fileHasChanged, good, fail, this._succesMessages[0]);
                NLogger.Debug(this._succesMessages[0]);
            }
            return false;
        }

        private Patcher.MainPattern FindAllPatterns(Patcher.Pattern pt)
        {
            if (this._isDisposed)
                throw new ObjectDisposedException("Object was disposed.");
            if (pt == null)
                throw new ArgumentNullException("Pattern");
            if (!pt.ValidPattern)
                return new Patcher.MainPattern(pt, (long[])null);
            long num1 = 0;
            int num2 = 0;
            int index1 = 0;
            uint num3 = 0;
            uint num4 = 0;
            byte[] buffer = new byte[this._bufferSize];
            int num5 = this._bufferSize;
            Patcher.Pattern.UniversalByte[] searchBytes = pt.SearchBytes;
            List<long> longList = new List<long>();
            if (this._isDisposed)
                throw new ObjectDisposedException("Object was disposed.");
            this._fs.Position = 0L;
            while (num5 > 0)
            {
                if (this._isDisposed)
                    throw new ObjectDisposedException("Object was disposed.");
                long position = this._fs.Position;
                num5 = this._fs.Read(buffer, 0, buffer.Length);
                for (int index2 = 0; index2 < num5; ++index2)
                {
                    if (searchBytes[index1].Act == Patcher.Pattern.UniversalByte.Action.Skip || (int)buffer[index2] == (int)searchBytes[index1].B)
                    {
                        if (index1 == 0)
                        {
                            num1 = position;
                            num2 = index2;
                        }
                        if (searchBytes.Length - 1 == index1)
                        {
                            index1 = 0;
                            if ((int)num3 == (int)pt.ReplaceAfter)
                            {
                                ++num4;
                                longList.Add(num1 + (long)num2);
                                if (pt.BrakAfter > 0U && (int)num4 == (int)pt.BrakAfter)
                                    return new Patcher.MainPattern(pt, longList.ToArray());
                            }
                            else
                                ++num3;
                        }
                        else
                            ++index1;
                    }
                    else if (index1 > 0)
                    {
                        index1 = 0;
                        index2 = num2;
                        if (num1 != position)
                        {
                            this._fs.Position = num1;
                            num5 = this._fs.Read(buffer, 0, buffer.Length);
                        }
                    }
                }
            }
            if (num4 <= 0U)
                return new Patcher.MainPattern(pt, (long[])null);
            return new Patcher.MainPattern(pt, longList.ToArray());
        }

        private bool ReplaceAllPatterns(Patcher.MainPattern pt)
        {
            if (!pt.Ptrn.ReplaceImmediately || this._isDisposed)
                return false;
            int num1 = 0;
            long[] streamPosition = pt.StreamPosition;
            Patcher.Pattern.UniversalByte[] replaceBytes = pt.Ptrn.ReplaceBytes;
            byte[] buffer = new byte[pt.Ptrn.SearchBytes.Length];
            foreach (long num2 in streamPosition)
            {
                if (this._isDisposed)
                    return false;
                bool flag = false;
                this._fs.Position = num2;
                this._fs.Read(buffer, 0, buffer.Length);
                for (int index = 0; index < buffer.Length; ++index)
                {
                    if (replaceBytes[index].Act > Patcher.Pattern.UniversalByte.Action.Skip && (int)buffer[index] != (int)replaceBytes[index].B)
                    {
                        buffer[index] = replaceBytes[index].B;
                        flag = true;
                    }
                }
                if (flag)
                {
                    this._fs.Position = num2;
                    this._fs.Write(buffer, 0, buffer.Length);
                    ++num1;
                }
            }
            return true;
        }

        private FileStream CreateStream(string fileName)
        {
            File.GetAttributes(fileName);
            FileAttributes fileAttributes = FileAttributes.Normal;
            File.SetAttributes(fileName, fileAttributes);
            FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite);
            if (fileStream.Length < 16L)
            {
                fileStream.Close();
                throw new IOException("File is too short! (posible damaged).");
            }
            return fileStream;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this._isDisposed)
                return;
            if (disposing && this._fs != null)
                this._fs.Dispose();
            this._fs = (FileStream)null;
            this._isDisposed = true;
        }

        public class PatcherState
        {
            public readonly uint Fail;
            public readonly bool FileHasChanged;
            public readonly uint Good;
            public readonly string StringState;

            public PatcherState(bool fileHasChanged, uint good, uint fail, string stringState = "")
            {
                this.FileHasChanged = fileHasChanged;
                this.Good = good;
                this.Fail = fail;
                this.StringState = stringState;
            }
        }

        public struct MainPattern
        {
            public readonly Patcher.Pattern Ptrn;
            public readonly long[] StreamPosition;

            public int SuccessfullyFound
            {
                get
                {
                    if (this.StreamPosition == null)
                        return 0;
                    return this.StreamPosition.Length;
                }
            }

            public bool Success
            {
                get
                {
                    if (this.StreamPosition == null)
                        return false;
                    if (this.Ptrn.BrakAfter < 1U && this.StreamPosition.Length != 0)
                        return true;
                    return (long)(this.Ptrn.BrakAfter - this.Ptrn.ReplaceAfter) == (long)this.StreamPosition.Length;
                }
            }

            public MainPattern(Patcher.Pattern ptrn, long[] streamPosition)
            {
                if (ptrn == null)
                    throw new ArgumentNullException("Pattern.");
                this.Ptrn = ptrn;
                this.StreamPosition = streamPosition;
            }
        }

        public class Pattern
        {
            public readonly Patcher.Pattern.UniversalByte[] ReplaceBytes;
            public readonly Patcher.Pattern.UniversalByte[] SearchBytes;
            private readonly bool _valid;
            public uint BrakAfter;
            public uint ReplaceAfter;

            public bool ValidPattern
            {
                get
                {
                    if (this.BrakAfter > 0U && this.BrakAfter - this.ReplaceAfter < 1U)
                        return false;
                    return this._valid;
                }
            }

            public bool ReplaceImmediately { get; }

            public Pattern(string se, string rep, uint brakAfter = 1, uint replaceAfter = 0)
            {
                if (string.IsNullOrEmpty(se))
                    throw new ArgumentException("Input string null or empty.");
                this.BrakAfter = brakAfter;
                this.ReplaceAfter = replaceAfter;
                if (rep == null)
                {
                    this.SearchBytes = this.TryParse(se);
                    this.ReplaceImmediately = false;
                    this._valid = true;
                }
                else
                {
                    this.SearchBytes = this.TryParse(se);
                    this.ReplaceBytes = this.TryParse(rep);
                    this.ReplaceImmediately = true;
                    this._valid = true;
                }
            }

            private Patcher.Pattern.UniversalByte[] TryParse(string st)
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (char ch in st)
                {
                    if ((int)ch > 32 && (int)ch < (int)sbyte.MaxValue)
                        stringBuilder.Append(ch);
                }
                string upper = stringBuilder.ToString().ToUpper();
                if (upper.Length % 2 != 0)
                    throw new Exception("String of byte must be power of two.");
                bool flag = false;
                Patcher.Pattern.UniversalByte[] universalByteArray = new Patcher.Pattern.UniversalByte[upper.Length / 2];
                int index = 0;
                int startIndex = 0;
                while (startIndex < upper.Length)
                {
                    string s = upper.Substring(startIndex, 2);
                    if (s == "??")
                    {
                        universalByteArray[index].Act = Patcher.Pattern.UniversalByte.Action.Skip;
                    }
                    else
                    {
                        universalByteArray[index].Act = Patcher.Pattern.UniversalByte.Action.Normal;
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
                public Patcher.Pattern.UniversalByte.Action Act;
                public byte B;

                public enum Action
                {
                    Skip,
                    Normal,
                }
            }
        }
    }
}
