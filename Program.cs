using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace nCommentChecker
{
    class Program
    {
        public static List<Adatok> eredmenynekNagyonMegfeleloKilistazottAnyamkinja = new List<Adatok>();
        public static List<Adatok> rendezettUltimateLista = new List<Adatok>();
        static string[] temp;
        static string torrentNeve;
        static string glogalWebcim;
        static string globalDatum;
        static string cookie;
        static int oldalakSzama =1;
        static int q = 1;
        static Boolean listaNelkul; //az en defaultom 0 de pcrolandé true
        static Boolean kommenteloKereses;
        static Boolean kommenteloSzerintListazando;
        static Boolean anonim = false;
        static string kommentelo;
        public static int listaSorSzam;
        static string profileid;
        static string utolsoOldalLink;
        static int osszesTorrentSzam =0;
        static int aktualisTorrentSzam = 0;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("This programme needs parameters, please command the -h switch for further informations.");
                Environment.Exit(1);
            }
            for (var x = 0; x < args.Count(); x++)
            {
                switch (args[x].Trim())
                {
                    case "-w":
                        string kulcsSzo = args[x + 1];
                        glogalWebcim = "https://ncore.cc/torrents.php?tipus=all_own&mire=" + kulcsSzo + "&miben=name";
                        break;
                    case "-u":
                        string feltoltoNev = args[x + 1];
                        glogalWebcim = "https://ncore.cc/torrents.php?tipus=all_own&mire=" + feltoltoNev + "&miben=uploaded_by_nev";
                        break;
                    case "-d":
                        string datum = args[x + 1];
                        globalDatum = datum + " 0:00:00";
                        break;
                    case "-l":
                        listaNelkul = true; // ez itt eredetileg = true de pcrolandehoz = false
                        break;
                    case "-h":
                        Console.WriteLine("Tool created by Bookember.\n\nUsage:\n-w Looking for a keyword\n-u Looking for the uploader's name\n-d The input date (should be formated as YYYY-MM-DD)\n-l Makes the listing descending by the upload date of the torrents.\n-c It gives you the option, to search by commenters.\n-a If you are an anonim uploader and you would like to search to your own torrents, use this.\n\nExamples:\n-w sajt -d 2019-02-09\n-u bookember -d 2018-12-12\n-u pcroland -c Bookember -l\n-a -d 2019-03-15\n\nLetters are case sensitive especially in the case of -c switch! Please don't use -u -a and the -w switch at the same time!\nYou will need a cookies.txt with your nCore cookies, placed next to this .exe!\nTo export your cookies, I recommend to use this add-on: https://chrome.google.com/webstore/detail/cookiestxt/njabckikapfpffapmjgojcnbfjonfjfg \nv1.0.5");
                        Environment.Exit(1);
                        break;
                    case "-c":
                        kommenteloKereses = true;
                        kommentelo = args[x + 1];
                        break;
                    case "-a":
                        anonim = true;
                        break;
                }
            }
            string dir =System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string cookiePath = dir +  @"\cookies.txt";
            StreamReader cookieOlvas = new StreamReader(cookiePath);
            string sorer;
            string[] cookieTemp;
            while (!cookieOlvas.EndOfStream)
            {
                sorer = cookieOlvas.ReadLine() + ';';
                if (!sorer.Contains('#'))
                {
                    cookieTemp = sorer.Split('	');
                    string cookieSor = cookieTemp[5] + "=" + cookieTemp[6];
                    cookie = cookie + cookieSor;
                }
            }
            cookieOlvas.Close();
            DateTime dateTime = DateTime.UtcNow.Date;
            string maiDatum = Convert.ToString(dateTime).Substring(0, 12).Replace(".", "-").Replace(" ", ""); //a mai dátum YYYY-MM-DD formátumra való alakítása
            string maiiiDatum = maiDatum + Convert.ToString(dateTime).Substring(13);
            var client = new WebClient();
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36;)");
            client.Headers.Add(HttpRequestHeader.Cookie, cookie);
            //-a funkció innen
            if (anonim==true)
            {
                client.DownloadFile("https://ncore.cc/profile.php", "ncc_temp3.txt");
                StreamReader psw = new StreamReader("ncc_temp3.txt");
                string psor;
                while (!psw.EndOfStream)
                {
                    psor = psw.ReadLine();
                    if (psor.Contains("data-uid="))
                    {
                        string[] temp;
                        temp = psor.Split(' ');
                        string profileidj = new String (temp[3].Where(Char.IsDigit).ToArray());
                        profileid = profileidj;
                    }
                }
                psw.Close();
                string selfUpWeb = "https://ncore.cc/profile.php?id=" + profileid + "&action=torrents";
                client.DownloadFile(selfUpWeb, "ncc_temp4.txt");
                Console.WriteLine("Beginning...");
                Console.WriteLine();
                StreamReader ssq = new StreamReader("ncc_temp4.txt");
                string ssoq;
                while (!ssq.EndOfStream)
                {
                    ssoq = ssq.ReadLine();
                    if (!ssoq.Contains("var loading") && ssoq.Contains("torrents.php?action=") && ssoq.Contains("title="))
                    {
                        for (int a = 0; a < ssoq.Length; a++)
                        {
                            temp = ssoq.Split('"');
                        }
                        for (int b = 0; b < temp.Length; b++)
                        {
                            if (temp[b].Contains("torrents.php"))
                            {
                                osszesTorrentSzam = osszesTorrentSzam + 1 ;
                            }
                        }
                    }
                }
                ssq.Close();
                StreamReader ssr = new StreamReader("ncc_temp4.txt");
                string ssor;
                while (!ssr.EndOfStream)
                {
                    List<string> torrentWebcimLista = new List<string>();
                    ssor = ssr.ReadLine();
                    if (!ssor.Contains("var loading") && ssor.Contains("torrents.php?action=") && ssor.Contains("title="))
                    {
                        for (int a = 0; a < ssor.Length; a++)
                        {
                            temp = ssor.Split('"');
                        }
                        for (int b = 0; b < temp.Length; b++)
                        {
                            if (temp[b].Contains("torrents.php"))
                            {
                                torrentWebcimLista.Add(temp[b]);
                            }
                        }
                    }
                    for (int c = 0; c < torrentWebcimLista.Count; c++) ///ITT TÖLTI LE Xszer ahány oldalszám van és ezt fixelni kell
                    {
                        //százalék
                        string webcim = "https://ncore.cc/" + torrentWebcimLista[c];
                        client.DownloadFile(webcim, "ncc_temp2.txt");
                        StreamReader srx = new StreamReader("ncc_temp2.txt");
                        Boolean elsoTalaltTorrentNev = false;
                        while (!srx.EndOfStream)
                        {
                            string srxSor = srx.ReadLine();
                            int tempCurrentTorrentNumber = aktualisTorrentSzam;
                            if (srxSor.Contains("db</div>"))
                            {

                            }
                            if (srxSor.Contains("torrent_reszletek_cim"))
                            {
                                string[] anyadatMar;
                                anyadatMar = srxSor.Split('>');
                                torrentNeve = anyadatMar[1].Substring(0, anyadatMar[1].IndexOf("<", 0));
                                if (torrentNeve.Contains("&amp;"))
                                {
                                    torrentNeve = torrentNeve.Replace("&amp;", "&");

                                }
                                aktualisTorrentSzam++;
                                //megpróbálom beimplementálni az aktuális szám kiírását és nevét
                                if (elsoTalaltTorrentNev == false && tempCurrentTorrentNumber != aktualisTorrentSzam)
                                {
                                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                                    ClearCurrentConsoleLine();
                                    Console.WriteLine("[" + aktualisTorrentSzam + "/" + osszesTorrentSzam + "] - " + torrentNeve);
                                    elsoTalaltTorrentNev = true;
                                    tempCurrentTorrentNumber++;
                                }
                                //39
                            }
                            if (kommenteloKereses == true && srxSor.Contains("Írta:") && srxSor.Contains(kommentelo))
                            {
                                //akkor ehhez a torrenthez hozzászólt az a bizonyos kommentelő, csak ezeket a torrenteket kell kilistázni
                                kommenteloSzerintListazando = true;
                            }
                            if ((srxSor.Contains("&nbsp;201") || srxSor.Contains("&nbsp;202")) && srxSor.Contains("hsz_pont"))
                            {
                                //2019. 02. 07. 0:00:00
                                string kommentDatum = srxSor.Substring(srxSor.IndexOf(";", 0) + 1, 19);
                                TimeSpan datumKulonbseg = Convert.ToDateTime(maiiiDatum) - Convert.ToDateTime(kommentDatum);
                                TimeSpan datumKulonbseg2 = Convert.ToDateTime(maiDatum) - Convert.ToDateTime(globalDatum);
                                if (datumKulonbseg <= datumKulonbseg2)
                                {
                                    Adatok adat = new Adatok(); //osztály példányosítás, itt töltöm fel az egy pédányt az adott torrent adataival
                                    adat.listabaTorrentWebcimLista = "https://ncore.cc/t/" + torrentWebcimLista[c].Substring(31);
                                    adat.listtabaKommentDatum = kommentDatum;
                                    adat.listabaTorrentNeve = torrentNeve;
                                    //itt kéne eldönteni, hogy a lista tartalmazza-e
                                    Boolean eldontendo = false;
                                    Adatok tempAdat = new Adatok(); //-l kapcsolóhoz kell, hogy így is a legújabb komentet listázza, ha nem rendszerezzük a listát
                                    if (kommenteloKereses == true)
                                    {
                                        if (kommenteloSzerintListazando == true)
                                        {
                                            for (int i = 0; i < eredmenynekNagyonMegfeleloKilistazottAnyamkinja.Count; i++)
                                            {
                                                if (eredmenynekNagyonMegfeleloKilistazottAnyamkinja[i].listabaTorrentNeve == adat.listabaTorrentNeve)
                                                {
                                                    eldontendo = true;
                                                    listaSorSzam = i;
                                                }
                                            }
                                            if (eldontendo == true)
                                            {
                                                eredmenynekNagyonMegfeleloKilistazottAnyamkinja.RemoveAt(listaSorSzam);
                                                eredmenynekNagyonMegfeleloKilistazottAnyamkinja.Add(adat);
                                            }
                                            else
                                            {
                                                if (listaNelkul == true)
                                                {
                                                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                                                    ClearCurrentConsoleLine();
                                                    tempAdat.listabaTorrentNeve = adat.listabaTorrentNeve;
                                                    tempAdat.listabaTorrentWebcimLista = adat.listabaTorrentWebcimLista;
                                                    tempAdat.listtabaKommentDatum = adat.listtabaKommentDatum;
                                                    Console.WriteLine(tempAdat.listabaTorrentWebcimLista + " | " + tempAdat.listtabaKommentDatum + " | " + tempAdat.listabaTorrentNeve);
                                                    Console.WriteLine();
                                                }
                                                eredmenynekNagyonMegfeleloKilistazottAnyamkinja.Add(adat); //majd berakom a listába
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < eredmenynekNagyonMegfeleloKilistazottAnyamkinja.Count; i++)
                                        {
                                            if (eredmenynekNagyonMegfeleloKilistazottAnyamkinja[i].listabaTorrentNeve == adat.listabaTorrentNeve)
                                            {
                                                eldontendo = true;
                                                listaSorSzam = i;
                                            }
                                        }
                                        if (eldontendo == true)
                                        {
                                            eredmenynekNagyonMegfeleloKilistazottAnyamkinja.RemoveAt(listaSorSzam);
                                            eredmenynekNagyonMegfeleloKilistazottAnyamkinja.Add(adat);
                                        }
                                        else
                                        {
                                            if (listaNelkul == true)
                                            {
                                                Console.SetCursorPosition(0, Console.CursorTop - 1);
                                                ClearCurrentConsoleLine();
                                                tempAdat.listabaTorrentNeve = adat.listabaTorrentNeve;
                                                tempAdat.listabaTorrentWebcimLista = adat.listabaTorrentWebcimLista;
                                                tempAdat.listtabaKommentDatum = adat.listtabaKommentDatum;
                                                Console.WriteLine(tempAdat.listabaTorrentWebcimLista + " | " + tempAdat.listtabaKommentDatum + " | " + tempAdat.listabaTorrentNeve);
                                                Console.WriteLine();
                                            }
                                            eredmenynekNagyonMegfeleloKilistazottAnyamkinja.Add(adat); //majd berakom a listába
                                        }
                                    }
                                }
                            }
                        }
                        srx.Close();
                        kommenteloSzerintListazando = false;
                    }
                }
                ssr.Close();
            }
            else
            {
                // most megnézem ki tudom-e szedni a torrentek számát

                string sorLinkes;
                client.DownloadFile(glogalWebcim.Substring(0, glogalWebcim.IndexOf('?')) + "?oldal=" + q + '&' + glogalWebcim.Substring(glogalWebcim.IndexOf('?') + 1), "ncc_temp.txt");
                StreamReader srLinkes = new StreamReader("ncc_temp.txt");
                while (!srLinkes.EndOfStream)
                {
                    sorLinkes = srLinkes.ReadLine();
                    if (sorLinkes.Contains("<strong>Utolsó</strong>"))
                    {
                        utolsoOldalLink = "https://ncore.cc" + sorLinkes.Split('|').Last().Substring(10);
                        utolsoOldalLink = utolsoOldalLink.Substring(0, utolsoOldalLink.LastIndexOf('"'));
                        break;
                    }
                }
                srLinkes.Close();
                string sorLinkes2;
                client.DownloadFile(utolsoOldalLink, "ncc_temp.txt"); 
                StreamReader srLinkes2 = new StreamReader("ncc_temp.txt");
                while (!srLinkes2.EndOfStream)
                {
                    sorLinkes2 = srLinkes2.ReadLine();
                    if (sorLinkes2.Contains("<strong>Első</strong></a>"))
                    {
                        sorLinkes2 = sorLinkes2.Substring(sorLinkes2.LastIndexOf('-')+1);
                        osszesTorrentSzam = Convert.ToInt32(sorLinkes2 = sorLinkes2.Substring(0, sorLinkes2.IndexOf('<')));
                        break;
                    }
                }
                srLinkes2.Close();
                //IGEN, kiszedtem!
                //IDE JÖN MAJD BE AZ EGÉSZ ELJÁRÁS AMI NEM -a
                while (q <= oldalakSzama)
                {
                    List<string> torrentWebcimLista = new List<string>();
                    client.DownloadFile(glogalWebcim.Substring(0, glogalWebcim.IndexOf('?')) + "?oldal=" + q + '&' + glogalWebcim.Substring(glogalWebcim.IndexOf('?') + 1), "ncc_temp.txt"); //első letöltés INNENTŐL KÉNE FOR CIKLUSBA TENNI, HOGY X LEGYEN .php?oldal=x
                    if (q==1)
                    {
                        Console.WriteLine("Beginning...");
                        Console.WriteLine();
                    }
                    //Console.WriteLine(glogalWebcim.Substring(0, glogalWebcim.IndexOf('?')) + "?oldal=" + q + '&' + glogalWebcim.Substring(glogalWebcim.IndexOf('?') + 1));
                    string sor;
                    StreamReader sr = new StreamReader("ncc_temp.txt");
                    while (!sr.EndOfStream)
                    {
                        sor = sr.ReadLine();
                        if (!sor.Contains("var loading") && sor.Contains("torrents.php?action=") && sor.Contains("title="))
                        {
                            for (int a = 0; a < sor.Length; a++)
                            {
                                temp = sor.Split('"');
                            }
                            for (int b = 0; b < temp.Length; b++)
                            {
                                if (temp[b].Contains("torrents.php"))
                                {
                                    torrentWebcimLista.Add(temp[b]);
                                }
                            }
                        }
                        if (sor.Contains("?oldal="))
                        {
                            string keresendoSzar = "?oldal=";
                            string asd = sor.Substring(sor.LastIndexOf(keresendoSzar)).Substring(7);
                            oldalakSzama = Convert.ToInt32(asd.Substring(0, asd.IndexOf('&')));
                        }
                    }
                    for (int c = 0; c < torrentWebcimLista.Count; c++) ///ITT TÖLTI LE Xszer ahány oldalszám van és ezt fixelni kell
                    {
                        string webcim = "https://ncore.cc/" + torrentWebcimLista[c];
                        client.DownloadFile(webcim, "ncc_temp2.txt");
                        StreamReader srx = new StreamReader("ncc_temp2.txt");
                        while (!srx.EndOfStream)
                        {
                            int tempCurrentTorrentNumber = aktualisTorrentSzam;
                            string srxSor = srx.ReadLine();
                            if (srxSor.Contains("torrent_reszletek_cim"))
                            {
                                string[] anyadatMar;
                                anyadatMar = srxSor.Split('>');
                                torrentNeve = anyadatMar[1].Substring(0, anyadatMar[1].IndexOf("<", 0));
                                if (torrentNeve.Contains("&amp;"))
                                {
                                    torrentNeve = torrentNeve.Replace("&amp;", "&");
                                }
                                aktualisTorrentSzam++;
                                //ITT VAN AZ & JELES BUG
                                //39
                            }
                            //megpróbálom beimplementálni az aktuális szám kiírását és nevét
                            if (tempCurrentTorrentNumber!=aktualisTorrentSzam)
                            {
                                Console.SetCursorPosition(0, Console.CursorTop - 1);
                                ClearCurrentConsoleLine();
                                Console.WriteLine("[" + aktualisTorrentSzam + "/" + osszesTorrentSzam + "] - " + torrentNeve);
                                tempCurrentTorrentNumber++;
                            }
                            if (kommenteloKereses == true && srxSor.Contains("Írta:") && srxSor.Contains(kommentelo))
                            {
                                //akkor ehhez a torrenthez hozzászólt az a bizonyos kommentelő, csak ezeket a torrenteket kell kilistázni
                                kommenteloSzerintListazando = true;
                            }
                            if ((srxSor.Contains("&nbsp;201") || srxSor.Contains("&nbsp;202")) && srxSor.Contains("hsz_pont"))
                            {
                                //2019. 02. 07. 0:00:00
                                string kommentDatum = srxSor.Substring(srxSor.IndexOf(";", 0) + 1, 19);
                                TimeSpan datumKulonbseg = Convert.ToDateTime(maiiiDatum) - Convert.ToDateTime(kommentDatum);
                                TimeSpan datumKulonbseg2 = Convert.ToDateTime(maiDatum) - Convert.ToDateTime(globalDatum);
                                if (datumKulonbseg <= datumKulonbseg2)
                                {
                                    Adatok adat = new Adatok(); //osztály példányosítás, itt töltöm fel az egy pédányt az adott torrent adataival
                                    adat.listabaTorrentWebcimLista = "https://ncore.cc/t/" + torrentWebcimLista[c].Substring(31);
                                    adat.listtabaKommentDatum = kommentDatum;
                                    adat.listabaTorrentNeve = torrentNeve;
                                    //itt kéne eldönteni, hogy a lista tartalmazza-e
                                    Boolean eldontendo = false;
                                    Adatok tempAdat = new Adatok(); //-l kapcsolóhoz kell, hogy így is a legújabb komentet listázza, ha nem rendszerezzük a listát
                                    if (kommenteloKereses == true)
                                    {
                                        if (kommenteloSzerintListazando == true)
                                        {
                                            for (int i = 0; i < eredmenynekNagyonMegfeleloKilistazottAnyamkinja.Count; i++)
                                            {
                                                if (eredmenynekNagyonMegfeleloKilistazottAnyamkinja[i].listabaTorrentNeve == adat.listabaTorrentNeve)
                                                {
                                                    eldontendo = true;
                                                    listaSorSzam = i;
                                                }
                                            }
                                            if (eldontendo == true)
                                            {
                                                eredmenynekNagyonMegfeleloKilistazottAnyamkinja.RemoveAt(listaSorSzam);
                                                eredmenynekNagyonMegfeleloKilistazottAnyamkinja.Add(adat);
                                            }
                                            else
                                            {
                                                if (listaNelkul == true)
                                                {
                                                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                                                    ClearCurrentConsoleLine();
                                                    tempAdat.listabaTorrentNeve = adat.listabaTorrentNeve;
                                                    tempAdat.listabaTorrentWebcimLista = adat.listabaTorrentWebcimLista;
                                                    tempAdat.listtabaKommentDatum = adat.listtabaKommentDatum;
                                                    Console.WriteLine(tempAdat.listabaTorrentWebcimLista + " | " + tempAdat.listtabaKommentDatum + " | " + tempAdat.listabaTorrentNeve);
                                                    Console.WriteLine();
                                                }
                                                eredmenynekNagyonMegfeleloKilistazottAnyamkinja.Add(adat); //majd berakom a listába
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < eredmenynekNagyonMegfeleloKilistazottAnyamkinja.Count; i++)
                                        {
                                            if (eredmenynekNagyonMegfeleloKilistazottAnyamkinja[i].listabaTorrentNeve == adat.listabaTorrentNeve)
                                            {
                                                eldontendo = true;
                                                listaSorSzam = i;
                                            }
                                        }
                                        if (eldontendo == true)
                                        {
                                            eredmenynekNagyonMegfeleloKilistazottAnyamkinja.RemoveAt(listaSorSzam);
                                            eredmenynekNagyonMegfeleloKilistazottAnyamkinja.Add(adat);
                                        }
                                        else
                                        {
                                            if (listaNelkul == true)
                                            {
                                                Console.SetCursorPosition(0, Console.CursorTop - 1);
                                                ClearCurrentConsoleLine();
                                                tempAdat.listabaTorrentNeve = adat.listabaTorrentNeve;
                                                tempAdat.listabaTorrentWebcimLista = adat.listabaTorrentWebcimLista;
                                                tempAdat.listtabaKommentDatum = adat.listtabaKommentDatum;
                                                Console.WriteLine(tempAdat.listabaTorrentWebcimLista + " | " + tempAdat.listtabaKommentDatum + " | " + tempAdat.listabaTorrentNeve);
                                                Console.WriteLine();
                                            }
                                            eredmenynekNagyonMegfeleloKilistazottAnyamkinja.Add(adat); //majd berakom a listába
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        srx.Close();
                        kommenteloSzerintListazando = false;
                    }
                    sr.Close();
                    q++;
                }
            }
            rendezettUltimateLista = eredmenynekNagyonMegfeleloKilistazottAnyamkinja.OrderByDescending(x => x.listtabaKommentDatum).ToList();
            if (listaNelkul==true)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                ClearCurrentConsoleLine();
            }
            else
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                ClearCurrentConsoleLine();
                for (int i = 0; i < rendezettUltimateLista.Count; i++)
                {
                    Console.WriteLine(rendezettUltimateLista[i].listabaTorrentWebcimLista + " | " + rendezettUltimateLista[i].listtabaKommentDatum + " | " + rendezettUltimateLista[i].listabaTorrentNeve);
                }
            }
            File.Delete("ncc_temp.txt");
            File.Delete("ncc_temp2.txt");
            File.Delete("ncc_temp3.txt");
            File.Delete("ncc_temp4.txt");
            Console.WriteLine("Done.");
        }
        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }
    }
}
