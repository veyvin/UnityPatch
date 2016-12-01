using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UnityPatch
{
    public class NLogger
    {
        private static List<NLogger.Message> _msgs = new List<NLogger.Message>();
        public static short MaxMessages = 8;
        private static short _errorState = 0;

        public static void Clear()
        {
            NLogger._msgs.Clear();
            NLogger._errorState = (short)0;
        }

        public static void LastXMessages()
        {
            if (NLogger._msgs.Count < 1)
                return;
            StringBuilder stringBuilder = new StringBuilder();
            int count = NLogger._msgs.Count;
            if (count > (int)NLogger.MaxMessages)
            {
                for (int index = count - (int)NLogger.MaxMessages; index < count; ++index)
                    stringBuilder.Append(NLogger._msgs[index].message).AppendLine();
            }
            else
            {
                foreach (NLogger.Message msg in NLogger._msgs)
                    stringBuilder.Append(msg.message).AppendLine();
            }
            NLogger.Publish(new NLogger.Message(stringBuilder.ToString(), NLogger._errorState));
        }

        public static void LastMessage()
        {
            if (NLogger._msgs.Count < 1)
                return;
            NLogger.Publish(NLogger._msgs[NLogger._msgs.Count - 1]);
        }

        public static void WriteLogToFile(string filename)
        {
            if (NLogger._msgs.Count < 1)
                return;
            try
            {
                File.WriteAllText(filename, NLogger.GetFullLog(true));
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show("IOException: Can't write log to file: " + ex.Message, "Patcher", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
            }
        }

        public static string GetFullLog(bool header = false)
        {
            if (NLogger._msgs.Count < 1)
                return (string)null;
            StringBuilder stringBuilder = new StringBuilder();
            if (header)
            {
                stringBuilder.Append("Patcher log file.").AppendLine();
                stringBuilder.Append("File format: {ErrorLevel} {Message},").AppendLine();
                stringBuilder.Append("0 = Ok,").AppendLine();
                stringBuilder.Append("1 = Information,").AppendLine();
                stringBuilder.Append("2 = Exclamation,").AppendLine();
                stringBuilder.Append("3 = Error").AppendLine();
            }
            foreach (NLogger.Message msg in NLogger._msgs)
                stringBuilder.Append(string.Format("{{0}} {{1}},", (object)msg.Level, (object)msg.message));
            return stringBuilder.ToString();
        }

        private static void Publish(NLogger.Message md)
        {
            if (md == null)
                return;
            switch (md.state)
            {
                case NLogger.Message.State.Debug:
                    int num1 = (int)MessageBox.Show(md.message, "Patcher");
                    break;
                case NLogger.Message.State.Information:
                    int num2 = (int)MessageBox.Show(md.message, "Patcher", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
                    break;
                case NLogger.Message.State.Exclamation:
                    int num3 = (int)MessageBox.Show(md.message, "Patcher", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    break;
                case NLogger.Message.State.Error:
                    int num4 = (int)MessageBox.Show(md.message, "Patcher", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
                    break;
            }
        }

        internal static void Debug(string ms)
        {
            NLogger._msgs.Add(new NLogger.Message(ms, (short)0));
        }

        internal static void Inf(string ms)
        {
            NLogger._msgs.Add(new NLogger.Message(ms, (short)1));
            if ((int)NLogger._errorState >= 1)
                return;
            NLogger._errorState = (short)1;
        }

        internal static void Warn(string ms)
        {
            NLogger._msgs.Add(new NLogger.Message(ms, (short)2));
            if ((int)NLogger._errorState >= 2)
                return;
            NLogger._errorState = (short)2;
        }

        internal static void Error(string ms)
        {
            NLogger._msgs.Add(new NLogger.Message(ms, (short)3));
            if ((int)NLogger._errorState >= 3)
                return;
            NLogger._errorState = (short)3;
        }

        private class Message
        {
            public readonly string message;
            public readonly NLogger.Message.State state;

            public int Level
            {
                get
                {
                    return (int)this.state;
                }
            }

            public Message(string ms, short st = 0)
            {
                this.message = ms;
                this.state = (NLogger.Message.State)st;
            }

            public enum State
            {
                Debug,
                Information,
                Exclamation,
                Error,
            }
        }
    }
}
