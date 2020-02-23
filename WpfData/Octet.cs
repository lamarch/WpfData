using System.CodeDom;
using System.Runtime.CompilerServices;

namespace WpfData
{
    public class Octet
    {
        const long coef = 1024;

        static readonly string[] suffixes = new[]
        {
            "B",
            "KB",
            "MB",
            "GB",
            "TB"
        };

        long octets;

        Octet(long octets)
        {
            this.octets = octets;
        }

        public Octet ( )
        {
            this.octets = 0;
        }

        public static Octet operator + ( Octet a, Octet b )
        {
            return Octet.FromOctet(a.GetOctets() + b.GetOctets());
        }

        public static Octet operator - (Octet a, Octet b)
        {
            return Octet.FromOctet(a.GetOctets() - b.GetOctets());
        }

        public static Octet operator * (Octet a, long b)
        {
            return Octet.FromOctet(a.GetOctets() * b);
        }

        public static double operator * (Octet a, Octet b)
        {
            return a.GetOctets() * (double)b.GetOctets();
        }

        public static Octet operator / (Octet a, long b)
        {
            return Octet.FromOctet(a.GetOctets() / b);
        }

        public static double operator / (Octet a, Octet b)
        {
            return a.GetOctets() / (double)b.GetOctets();
        }

        public static bool operator > (Octet a, Octet b)
        {
            return a.GetOctets() > b.GetOctets();
        }

        public static bool operator < (Octet a, Octet b)
        {
            return a.GetOctets() > b.GetOctets();
        }

        public static bool operator >= (Octet a, Octet b)
        {
            return a.GetOctets() >= b.GetOctets();
        }

        public static bool operator <= (Octet a, Octet b)
        {
            return a.GetOctets() > b.GetOctets();
        }

        public static Octet FromOctet (long octet) => new Octet(octet);
        public static Octet FromKilo (long kilo) => Octet.FromOctet(kilo * coef);
        public static Octet FromMega (long mega) => Octet.FromKilo(mega * coef);
        public static Octet FromGiga (long giga) => Octet.FromMega(giga * coef);
        public static Octet FromTera (long tera) => Octet.FromGiga(tera * coef);

        public long GetOctets ( ) => octets;
        public double GetKilo ( ) => GetOctets() / coef;
        public double GetMega ( ) => GetKilo() / coef;
        public double GetGiga ( ) => GetMega() / coef;
        public double GetTera ( ) => GetGiga() / coef;

        public override string ToString ( ) => ToString(2, true);

        public string ToString (bool suffixe) => ToString(2, suffixe);
        public string ToString ( int decimalCount, bool suffixe = true )
        {
            decimal octets = (decimal)this.octets;
            decimal dCoef = (decimal)coef;
            if(octets < 0 )
            {
                return "- " + FromOctet((long)-octets).ToString();
            }

            int index = 0;
            while(octets >= dCoef )
            {
                octets /= (decimal)dCoef;
                index++;
            }

            return string.Format("{0:n" + decimalCount + "} {1}", octets, suffixes[index]);
        }
    }
}
