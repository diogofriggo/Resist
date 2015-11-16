using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;



namespace Resist
{
    class Program
    {
        static void Main(string[] args)
        {
            string readText = File.ReadAllText("C:/Users/Panda/Desktop/report.txt");

            //PROCESSANDO COMPRIMENTOS
            var comprimentos = new List<string[]>();
            string startComprimentos = @"Element  Node i  Node j    Length        Beta (deg)  Section  Material  
";
            string endComprimentos = @"     
                                                                            
  Connections           Flexural Stiffness                     Torsion";
            string betweenComprimentos = getBetween(readText, startComprimentos, endComprimentos);

            using (StringReader reader = new StringReader(betweenComprimentos))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var fields = Regex.Split(line, @"\s{1,}");
                    var barra = fields[1];
                    var startNode = fields[2];
                    var endNode = fields[3];
                    var comprimento = double.Parse(fields[4], CultureInfo.InvariantCulture);
                    var section = fields[6];
                    var material = fields[7];
                    var item = new string[] { barra, startNode, endNode, comprimento.ToString(), section, material };
                    comprimentos.Add(item);
                    //Console.WriteLine("{0} {1} {2} {3}", item[0], item[1], item[2], item[3]);
                }
            }

            //PROCESSANDO FORCAS
            string startForces = @"Element  Node        Fx             Fy             Fz                   
";
            string endForces = @"               
                                                                            
  Internal End Moments (Note: Refers to local coordinates)";
            string betweenForces = getBetween(readText, startForces, endForces);

            var compressoes = new List<string[]>();
            var tracoes = new List<string[]>();
            var cisalhamentos = new List<string[]>();
            
            using (StringReader reader = new StringReader(betweenForces))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var fields = Regex.Split(line, @"\s{1,}");
                    var barra = fields[1];
                    var startNode = fields[2];
                    var endNode = Regex.Split(reader.ReadLine(), @"\s{1,}")[1];
                    var fx = double.Parse(fields[3], CultureInfo.InvariantCulture);
                    var fy = double.Parse(fields[4], CultureInfo.InvariantCulture);
                    var fz = double.Parse(fields[5], CultureInfo.InvariantCulture);
                    var item = new string[] { barra, startNode, endNode, fx.ToString(), fy.ToString(), fz.ToString() };
                    if (Math.Abs(fy) > 0 || Math.Abs(fz) > 0)
                    {
                        cisalhamentos.Add(item);
                        //Console.WriteLine("{0} {1} {2} {3} {4} {5} {6}", item[0], item[1], item[2], item[3], item[4], item[5], "Cisalhamento");
                    }
                    var compressao = fx > 0;
                    if (compressao)
                        compressoes.Add(item);
                    else
                        tracoes.Add(item);
                    //Console.WriteLine("{0} {1} {2} {3} {4} {5} {6}", item[0], item[1], item[2], item[3], item[4], item[5], compressao ? "C" : "T");
                }
            }

            //PROCESSANDO MOMENTOS
            var momentos = new List<string[]>();
            string startMomentos = @"Element  Node        Mx             My             Mz             B     
";
            string endMomentos = @"
                                                                            
  Plastic Hinge Information";
            string betweenMomentos = getBetween(readText, startMomentos, endMomentos);

            using (StringReader reader = new StringReader(betweenMomentos))
            {
                string line1, line2;
                while ((line1 = reader.ReadLine()) != null)
                {
                    line2 = reader.ReadLine();
                    var fields1 = Regex.Split(line1, @"\s{1,}");
                    var fields2 = Regex.Split(line2, @"\s{1,}");
                    var barra = fields1[1];
                    var startNode = fields1[2];
                    var endNode = fields2[1];

                    var mx1 = double.Parse(fields1[3], CultureInfo.InvariantCulture);
                    var mx2 = double.Parse(fields2[2], CultureInfo.InvariantCulture);
                    
                    var my1 = double.Parse(fields1[4], CultureInfo.InvariantCulture);
                    var my2 = double.Parse(fields2[3], CultureInfo.InvariantCulture);
                    
                    var mz1 = double.Parse(fields1[5], CultureInfo.InvariantCulture);
                    var mz2 = double.Parse(fields2[4], CultureInfo.InvariantCulture);
                    
                    var mx = Math.Max(Math.Abs(mx1), Math.Abs(mx2)).ToString();
                    var my = Math.Max(Math.Abs(my1), Math.Abs(my2)).ToString();
                    var mz = Math.Max(Math.Abs(mz1), Math.Abs(mz2)).ToString();

                    var item = new string[] { barra, startNode, endNode, mx, my, mz };
                    momentos.Add(item);
                    //Console.WriteLine("{0} {1} {2} {3}", my1, mz1, my2, mz2);
                    //Console.WriteLine("{0} {1} {2} {3} {4}", item[0], item[1], item[2], item[3], item[4]);
                }
            }

            //PROCESSANDO SECOES
            var sections = new List<string[]>();
            string startSections = @"Number     Area         Izz          Iyy           J           Cw       
";
            string endSections = @"  
                                                                            
  Part II:  Properties (continued)";
            string betweenSections = getBetween(readText, startSections, endSections);

            using (StringReader reader = new StringReader(betweenSections))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var fields = Regex.Split(line, @"\s{1,}");
                    var number = fields[1];
                    var area = fields[2];
                    var item = new string[] { number, area };
                    sections.Add(item);
                    //Console.WriteLine("{0} {1}", item[0], item[1]);
                }
            }

            //PROCESSANDO MATERIAIS
            var materials = new List<string[]>();
            string startMaterials = @"Number      E            v           Fy           Wt        Name        
";
            string endMaterials = @"        
                                                                            
(v)   Support Information";
            string betweenMaterials = getBetween(readText, startMaterials, endMaterials);

            using (StringReader reader = new StringReader(betweenMaterials))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var fields = Regex.Split(line, @"\s{1,}");
                    var number = fields[1];
                    var ec = double.Parse(fields[2], CultureInfo.InvariantCulture);
                    var ft = double.Parse(fields[4], CultureInfo.InvariantCulture);
                    var item = new string[] { number, ec.ToString(), ft.ToString() };
                    materials.Add(item);
                    //Console.WriteLine("{0} {1} {2}", item[0], item[1], item[2]);
                }
            }

            //SAIDA
            var output = new StringBuilder();
            
            //COMPRESSAO
            output.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}", "BARRA".PadRight(8), "L (mm)".PadRight(6), "Nc (N)".PadRight(8), "My(N.mm)".PadRight(8),
                "Mz(N.mm)".PadRight(8), "h adotado".PadRight(8), "k adotado".PadRight(10), "kl".PadRight(18), "Ncr (N)".PadRight(18), "S/R".PadRight(18), "h novo".PadRight(8)));
            
            var hset = new double[] { 1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5, 5.5, 6, 6.5, 7, 7.5, 8, 8.5, 9, 9.5 }; 
                
            foreach (var compressao in compressoes)
            {
                var barra = compressao[0];
                var startNode = compressao[1];
                var endNode = compressao[2];
                var comprimento = double.Parse(comprimentos.Single(x => x[0] == barra)[3]);
                var nc = double.Parse(compressao[3]);
                var my = double.Parse(momentos.Single(x => x[0] == barra)[4]);
                var mz = double.Parse(momentos.Single(x => x[0] == barra)[5]);
                var inicio = string.Format("{0} {1};{2}", barra, startNode, endNode);

                var section = comprimentos.Single(x => x[0] == barra)[4];
                var hadotado = Math.Sqrt(double.Parse(sections.Single(x => x[0] == section)[1], CultureInfo.InvariantCulture));
                var material = comprimentos.Single(x => x[0] == barra)[5];
                double k = 0.65;
                double ec = double.Parse(materials.Single(x => x[0] == material)[1], CultureInfo.InvariantCulture);
                double fc = double.Parse(materials.Single(x => x[0] == material)[2], CultureInfo.InvariantCulture);
                var hnovo = BestCrossSectionCompressao(hset, nc, my, mz, comprimento, k, fc, ec);
                var sr = getSrCompressao(hadotado, nc, my, mz, comprimento, k, fc, ec);
                var kl = getKl(hadotado, comprimento, k);
                var ncr = getNcr(hadotado, comprimento, k, ec);

                string saida = string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}", inicio.PadRight(8), comprimento.ToString().PadRight(6), nc.ToString().PadRight(8),
                    my.ToString().PadRight(8), mz.ToString().PadRight(8), hadotado.ToString().PadRight(8), k.ToString().PadRight(10), kl.ToString().PadRight(18), ncr.ToString().PadRight(18), 
                    sr.ToString().PadRight(18), hnovo.ToString().PadRight(8));
                output.AppendLine(saida);
                //Console.WriteLine(saida);
            }

            //TRACAO
            output.AppendLine();
            output.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", "BARRA".PadRight(8), "L (mm)".PadRight(6), "Nt (N)".PadRight(8), "My(N.mm)".PadRight(8), "Mz(N.mm)".PadRight(8),
                "h adotado".PadRight(8), "S/R".PadRight(18), "h novo".PadRight(8)));

            foreach (var tracao in tracoes)
            {
                var barra = tracao[0];
                var startNode = tracao[1];
                var endNode = tracao[2];
                var comprimento = double.Parse(comprimentos.Single(x => x[0] == barra)[3]);
                var nt = Math.Abs(double.Parse(tracao[3]));
                var my = double.Parse(momentos.Single(x => x[0] == barra)[4]);
                var mz = double.Parse(momentos.Single(x => x[0] == barra)[5]);
                var inicio = string.Format("{0} {1};{2}", barra, startNode, endNode);

                var section = comprimentos.Single(x => x[0] == barra)[4];
                var hadotado = Math.Sqrt(double.Parse(sections.Single(x => x[0] == section)[1], CultureInfo.InvariantCulture));
                
                var material = comprimentos.Single(x => x[0] == barra)[5];
                double et = double.Parse(materials.Single(x => x[0] == material)[1], CultureInfo.InvariantCulture);
                double ft = double.Parse(materials.Single(x => x[0] == material)[2], CultureInfo.InvariantCulture);
                var hnovo = BestCrossSectionTracao(hset, nt, my, mz, comprimento, ft);
                //Console.WriteLine("{0} {1} {2} {3} {4}", nt, my, mz, comprimento, ft);
                var sr = getSrTracao(hadotado, nt, my, mz, comprimento, ft);
                string saida = string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", inicio.PadRight(8), comprimento.ToString().PadRight(6), nt.ToString().PadRight(8),
                    my.ToString().PadRight(8), mz.ToString().PadRight(8), hadotado.ToString().PadRight(8), sr.ToString().PadRight(18), hnovo.ToString().PadRight(8));
                output.AppendLine(saida);
                //Console.WriteLine(saida);
            }

            //CISALHAMENTO
            output.AppendLine();
            output.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}", "BARRA".PadRight(8), "Vy (N)".PadRight(12), "Vz (N)".PadRight(12), "T (N.mm)".PadRight(12), "h adotado"));

            foreach (var cis in cisalhamentos)
            {
                var barra = cis[0];
                var startNode = cis[1];
                var endNode = cis[2];
                var vy = Math.Abs(double.Parse(cis[4])).ToString();
                var vz = Math.Abs(double.Parse(cis[5])).ToString();
                var mx = double.Parse(momentos.Single(x => x[0] == barra)[3]);
                var inicio = string.Format("{0} {1};{2}", barra, startNode, endNode);

                var section = comprimentos.Single(x => x[0] == barra)[4];
                var hadotado = Math.Sqrt(double.Parse(sections.Single(x => x[0] == section)[1], CultureInfo.InvariantCulture));

                string saida = string.Format("{0}\t{1}\t{2}\t{3}\t{4}", inicio.PadRight(8), vy.PadRight(8), vz.PadRight(8), mx.ToString().PadRight(8), hadotado.ToString().PadRight(8));
                output.AppendLine(saida);
                //Console.WriteLine(saida);
            }

            //CALCULO DA MASSA
            double densidade = 2.943 * Math.Pow(10, -6);
            double volume = 0;
            foreach (var barra in comprimentos)
            {
                var comprimento = double.Parse(barra[3]);
                var section = barra[4];
                var area = double.Parse(sections.Single(x => x[0] == section)[1], CultureInfo.InvariantCulture);
                volume += area * comprimento;
            }

            output.AppendLine();
            output.AppendLine(string.Format("Massa: {0} gramas", densidade * volume * 1000 / 9.81));

            output.AppendLine();
            foreach (var h in hset)
            {
                var fz = "0.000000000000";
                output.AppendLine(string.Format("h = {0}\tArea = {1}\tIzz=Iyy = {2}\tJ = {3}\tZzz=Zyy = {4}", h.ToString().PadRight(4), Math.Pow(h, 2).ToString().PadRight(10),
                    (Math.Pow(h, 4) / 12).ToString(fz).PadRight(14), (Math.Pow(h, 4) / 6).ToString(fz).PadRight(14), (Math.Pow(h, 3) / 6).ToString(fz).PadRight(14)));
            }
            File.WriteAllText("C:/Users/Panda/Desktop/report_formatted.txt", output.ToString());
        }

        public static double BestCrossSectionTracao(double[] hset, double nt, double  my, double mz, double l, double ft)
        {
            double besth = 0;
            double bestsr = 0;
            foreach(var h in hset)
            {
                var sr = getSrTracao(h, nt, my, mz, l, ft);
                if(sr <= 1 && sr > bestsr)
                {
                    besth = h;
                    bestsr = sr;
                }
            }
            return besth;
        }

        public static double getSrTracao(double h, double nt, double  my, double mz, double l, double ft)
        {
            return nt / (Math.Pow(h, 2) * ft) + (my + mz + 2 * nt * l / 300) / (Math.Pow(h, 3) * ft / 6);
        }

        public static double BestCrossSectionCompressao(double[] hset, double nc, double my, double mz, double l, double k, double fc, double ec)
        {
            double besth = 0;
            double bestsr = 0;
            foreach(var h in hset)
            {
                //Console.WriteLine(h);
                var sr = getSrCompressao(h, nc, my, mz, l, k, fc, ec);
                if (sr <= 1 && sr > bestsr)
                {
                    besth = h;
                    bestsr = sr;
                }
            }
            return besth;
        }
        public static double getSrCompressao(double h, double nc, double my, double mz, double l, double k, double fc, double ec)
        {
            var kl = k * l * Math.Sqrt(12) / h;
            var ncr = Math.Pow(Math.PI / kl, 2) * ec * Math.Pow(h, 2);
            if (nc > ncr)
                return 0;
            var fator = 1 - nc / ncr;
            return nc / (Math.Pow(h, 2) * fc) + (my + mz + 2 * nc * l / 300) / (Math.Pow(h, 3) * fc * fator / 6);
        }

        public static double getNcr(double h, double l, double k, double ec)
        {
            return Math.Pow(Math.PI / getKl(h, l, k), 2) * ec * Math.Pow(h, 2);
        }

        public static double getKl(double h, double l, double k)
        {
            return k * l * Math.Sqrt(12) / h;
        }

        public static double BestCrossSectionCisalhamento(double[] hset, double nt, double my, double mz, double l, double ft)
        {
            double besth = 0;
            double bestsr = 0;
            foreach (var h in hset)
            {
                var sr = nt / (Math.Pow(h, 2) * ft) + (my + mz + 2 * nt * l / 300) / (Math.Pow(h, 3) * ft / 6);
                if (sr <= 1 && sr > bestsr)
                {
                    besth = h;
                    bestsr = sr;
                }
            }
            return besth;
        }

        public static string getBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        }

        public static double calculateMass(List<Bar> bars)
        {
            double density = 2.943 * Math.Pow(10, -6);
            return bars.Sum(x => x.Length * x.Section.Area) * density;
        }

        public class Bar
        {
            public double Length { get; set; }
            public Section Section;
        }

        public class Section
        {
            public int Number { get; set; }
            public double Area { get; set; }
        }
    }
}

//BARRA	L (mm)	Nt (N)	My(N.mm)	Mz(N.mm)

//BARRA	L (mm)	Nc (N)	My(N.mm)	Mz(N.mm)

//BARRA	Vy (N)	Vz (N)	T (N.mm)