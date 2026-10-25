using System.Net;
using System.Text;

#pragma warning disable SYSLIB0014

namespace FTP_Lesson
{
    internal class Program
    {
        static string server = "ftp://127.0.0.1/";
        static string user = "test";
        static string pass = "1234";

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            while (true)
            {
                Console.WriteLine("\n1 - Faylların siyahısı (Get)");
                Console.WriteLine("2 - Fayl yüklə (Post)");
                Console.WriteLine("3 - Fayl endir (Download)");
                Console.WriteLine("4 - Fayl sil (Delete)");
                Console.WriteLine("5 - Faylı yenilə (Put)");
                Console.WriteLine("0 - Çıxış");
                Console.Write("Seçim: ");
                string? secim = Console.ReadLine();

                try
                {
                    if (secim == "1") Siyahi();
                    else if (secim == "2") Yukle();
                    else if (secim == "3") Endir();
                    else if (secim == "4") Sil();
                    else if (secim == "5") Yenile();
                    else if (secim == "0") break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Xəta: " + ex.Message);
                }
            }
        }

        static FtpWebRequest Sorgu(string fayl, string metod)
        {
            var req = (FtpWebRequest)WebRequest.Create(server + fayl);
            req.Method = metod;
            req.Credentials = new NetworkCredential(user, pass);
            return req;
        }

        static void Siyahi()
        {
            var req = Sorgu("", WebRequestMethods.Ftp.ListDirectory);
            using var res = (FtpWebResponse)req.GetResponse();
            using var reader = new StreamReader(res.GetResponseStream());
            Console.WriteLine("--- Fayllar ---");
            Console.WriteLine(reader.ReadToEnd());
        }

        static void Yukle()
        {
            Console.Write("Yüklənəcək faylın yolu: ");
            string yol = Console.ReadLine() ?? "";

            var req = Sorgu(Path.GetFileName(yol), WebRequestMethods.Ftp.UploadFile);
            byte[] data = File.ReadAllBytes(yol);
            using (var stream = req.GetRequestStream())
                stream.Write(data, 0, data.Length);

            using var res = (FtpWebResponse)req.GetResponse();
            Console.WriteLine("Yükləndi: " + res.StatusDescription);
        }

        static void Endir()
        {
            Console.Write("Endiriləcək faylın adı: ");
            string ad = Console.ReadLine() ?? "";

            var req = Sorgu(ad, WebRequestMethods.Ftp.DownloadFile);
            using var res = (FtpWebResponse)req.GetResponse();
            using var stream = res.GetResponseStream();
            using var file = File.Create(ad);
            stream.CopyTo(file);
            Console.WriteLine("Endirildi: " + Path.GetFullPath(ad));
        }

        static void Sil()
        {
            Console.Write("Silinəcək faylın adı: ");
            string ad = Console.ReadLine() ?? "";

            var req = Sorgu(ad, WebRequestMethods.Ftp.DeleteFile);
            using var res = (FtpWebResponse)req.GetResponse();
            Console.WriteLine("Silindi: " + res.StatusDescription);
        }

        static void Yenile()
        {
            Console.Write("Yenilənəcək faylın adı: ");
            string ad = Console.ReadLine() ?? "";

            var list = Sorgu("", WebRequestMethods.Ftp.ListDirectory);
            string fayllar;
            using (var r = (FtpWebResponse)list.GetResponse())
            using (var reader = new StreamReader(r.GetResponseStream()))
                fayllar = reader.ReadToEnd();

            bool tapildi = false;
            foreach (var f in fayllar.Split('\n'))
            {
                if (f.Trim() == ad)
                {
                    tapildi = true;
                    break;
                }
            }
            if (!tapildi)
            {
                Console.WriteLine("Belə fayl serverdə yoxdur!");
                return;
            }

            Console.Write("Yeni məlumatı yazın: ");
            string yeniData = Console.ReadLine() ?? "";
            byte[] data = Encoding.UTF8.GetBytes(yeniData);

            var req = Sorgu(ad, WebRequestMethods.Ftp.UploadFile);
            using (var stream = req.GetRequestStream())
                stream.Write(data, 0, data.Length);

            using var res = (FtpWebResponse)req.GetResponse();
            Console.WriteLine("Yeniləndi: " + res.StatusDescription);
        }
    }
}