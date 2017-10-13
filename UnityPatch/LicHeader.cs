using System.Collections.Generic;
using System.Linq;

namespace UnityPatch
{
    public class LicHeader
    {
        public static LicSettings PropLicSettings { get; set; } = new LicSettings();

        public static int[] ReadAll()
        {
            var source = new List<int>();
            switch (PropLicSettings.Type)
            {
                case 0:
                    source.Add(0);
                    source.Add(1);
                    source.Add(16);
                    break;
                case 1:
                    source.Add(0);
                    source.Add(1);
                    break;
                case 2:
                    source.Add(62);
                    break;
            }
            if (PropLicSettings.Team)
                source.Add(2);
            switch (PropLicSettings.IPhone)
            {
                case 0:
                    source.Add(3);
                    source.Add(4);
                    source.Add(9);
                    break;
                case 1:
                    source.Add(3);
                    source.Add(9);
                    break;
            }
            if (PropLicSettings.Xbox)
            {
                source.Add(5);
                source.Add(33);
                source.Add(11);
            }
            if (PropLicSettings.PlayStation)
            {
                source.Add(6);
                source.Add(10);
                source.Add(30);
                source.Add(31);
                source.Add(32);
            }
            if (PropLicSettings.Wii)
            {
                source.Add(23);
                source.Add(36);
            }
            if (PropLicSettings.Nin)
            {
                source.Add(39);
                source.Add(35);
            }
            if (PropLicSettings.NRelease)
                source.Add(61);
            if (PropLicSettings.Educt)
                source.Add(63);
            switch (PropLicSettings.Android)
            {
                case 0:
                    source.Add(12);
                    source.Add(13);
                    break;
                case 1:
                    source.Add(12);
                    break;
            }
            switch (PropLicSettings.Flash)
            {
                case 0:
                    source.Add(14);
                    source.Add(15);
                    break;
                case 1:
                    source.Add(14);
                    break;
            }
            switch (PropLicSettings.WinStore)
            {
                case 0:
                    source.Add(19);
                    source.Add(20);
                    source.Add(21);
                    source.Add(26);
                    break;
                case 1:
                    source.Add(19);
                    break;
            }
            switch (PropLicSettings.SamsungTv)
            {
                case 0:
                    source.Add(24);
                    source.Add(25);
                    source.Add(34);
                    break;
                case 1:
                    source.Add(24);
                    source.Add(34);
                    break;
            }
            switch (PropLicSettings.Blackberry)
            {
                case 0:
                    source.Add(17);
                    source.Add(18);
                    source.Add(28);
                    break;
                case 1:
                    source.Add(17);
                    source.Add(28);
                    break;
            }
            switch (PropLicSettings.Tizen)
            {
                case 0:
                    source.Add(33);
                    source.Add(34);
                    source.Add(29);
                    break;
                case 1:
                    source.Add(33);
                    source.Add(29);
                    break;
            }
            source.Sort();
            return source.Distinct().ToArray();
        }

        public class LicSettings
        {
            public bool Nin = true;
            public bool PlayStation = true;
            public bool Team = true;
            public int Type = 1;
            public bool Wii = true;
            public bool Xbox = true;
            public int Android;
            public int Blackberry;
            public bool Educt;
            public int Flash;
            public int IPhone;
            public bool NRelease;
            public int SamsungTv;
            public int Tizen;
            public int WinStore;
        }
    }
}