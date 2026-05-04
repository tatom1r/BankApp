// ============================================================
//  APLICATIE: Sistem Bancar
//  IERARHIE:  Cont (clasa de baza)
//                └─ ContEconomii (clasa derivata - nivel 2)
//                └─ ContCurent   (clasa derivata - nivel 2)
// ============================================================

using System;
using System.Collections.Generic;
using System.IO;

// ════════════════════════════════════════════════════════════
//  TRANZACTIE  –  inregistrare istorica
// ════════════════════════════════════════════════════════════
class Tranzactie
{
    public string Tip      { get; }
    public double Suma     { get; }
    public string Descriere{ get; }
    public DateTime Data   { get; }

    public Tranzactie(string tip, double suma, string descriere = "")
    {
        Tip      = tip;
        Suma     = suma;
        Descriere= descriere;
        Data     = DateTime.Now;
    }

    public override string ToString()
    {
        string desc = string.IsNullOrEmpty(Descriere) ? "" : $" | {Descriere}";
        return $"  [{Data:dd.MM.yyyy HH:mm}] {Tip,-14} {Suma,10:F2} RON{desc}";
    }
}

// ════════════════════════════════════════════════════════════
//  CLASA DE BAZA  –  Cont
// ════════════════════════════════════════════════════════════
class Cont
{
    // ── CONST si READONLY ───────────────────────────────────
    public const string BANCA = "MyBank SRL";          // const: valoare fixa la compilare
    public readonly string IBAN;                        // readonly: setata o singura data, in constructor

    // ── CAMP STATIC ─────────────────────────────────────────
    private static int totalConturi = 0;               // apartine clasei, nu unui obiect

    // ── CAMPURI de instanta ──────────────────────────────────
    protected string titular;                          // protected = vizibil si in clasele derivate
    protected double sold;
    protected List<Tranzactie> tranzactii = new();

    // ── CONSTRUCTOR STATIC ──────────────────────────────────
    // Rulat O SINGURA DATA, inainte de orice obiect
    static Cont()
    {
        totalConturi = 0;
        Console.WriteLine("[static constructor] Clasa Cont a fost initializata.\n");
    }

    // ── CONSTRUCTOR DE INSTANTA ─────────────────────────────
    public Cont(string titular, double soldInitial)
    {
        this.titular = titular;
        this.sold = soldInitial;
        this.IBAN = "RO" + (++totalConturi).ToString("D10");
    }

    // ── CONSTRUCTOR DE COPIERE ──────────────────────────────
    public Cont(Cont altCont)
    {
        this.titular = altCont.titular;
        this.sold = altCont.sold;
        this.IBAN = "RO" + (++totalConturi).ToString("D10"); // IBAN nou, unic
    }

    // ── PROPRIETATI (get + set) ──────────────────────────────
    public string Titular
    {
        get { return titular; }
        set { titular = value; }
    }

    public double Sold
    {
        get { return sold; }
        protected set { sold = value; }   // doar clasele derivate pot seta direct
    }

    public IReadOnlyList<Tranzactie> Tranzactii => tranzactii.AsReadOnly();

    // ── METODA STATICA ──────────────────────────────────────
    public static int GetTotalConturi() => totalConturi;

    // ── METODA VIRTUALA (polimorfism) ───────────────────────
    // virtual = poate fi suprascris in clasele derivate
    public virtual void AfiseazaInfo()
    {
        Console.WriteLine($"  Banca   : {BANCA}");
        Console.WriteLine($"  IBAN    : {IBAN}");
        Console.WriteLine($"  Titular : {titular}");
        Console.WriteLine($"  Sold    : {sold:F2} RON");
    }

    // ── SUPRAINCARCARI (overloading) ─────────────────────────
    // Aceeasi denumire, parametri diferiti
    public void Depune(double suma)
    {
        sold += suma;
        tranzactii.Add(new Tranzactie("Depunere", suma));
        Console.WriteLine($"  + Depus {suma:F2} RON. Sold nou: {sold:F2} RON");
    }

    public void Depune(double suma, string descriere)
    {
        sold += suma;
        tranzactii.Add(new Tranzactie("Depunere", suma, descriere));
        Console.WriteLine($"  + Depus {suma:F2} RON ({descriere}). Sold nou: {sold:F2} RON");
    }

    public virtual bool Retrage(double suma)
    {
        if (sold < suma)
        {
            Console.WriteLine($"  ! Fonduri insuficiente. Sold: {sold:F2} RON");
            return false;
        }
        sold -= suma;
        tranzactii.Add(new Tranzactie("Retragere", suma));
        Console.WriteLine($"  - Retras {suma:F2} RON. Sold nou: {sold:F2} RON");
        return true;
    }

    // ── CONVERSIE EXPLICITA: Cont -> string ──────────────────
    // Permite: string s = (string)cont;
    public static explicit operator string(Cont c)
    {
        // Format folosit pentru salvare in fisier:  Tip,Titular,Sold,[extra]
        return $"Cont,{c.titular},{c.sold}";
    }

    public override string ToString()
        => $"[Cont] {titular} | IBAN: {IBAN} | Sold: {sold:F2} RON";
}


// ════════════════════════════════════════════════════════════
//  CLASA DERIVATA (nivel 2) – ContEconomii  : Cont
// ════════════════════════════════════════════════════════════
class ContEconomii : Cont
{
    // Camp propriu clasei derivate
    private double rataDobanda;   // procent anual

    // ── Constructor de instanta ─────────────────────────────
    public ContEconomii(string titular, double soldInitial, double rataDobanda)
        : base(titular, soldInitial)   // apelam constructorul din Cont
    {
        this.rataDobanda = rataDobanda;
    }

    // ── Constructor de copiere ──────────────────────────────
    public ContEconomii(ContEconomii altCont)
        : base(altCont)
    {
        this.rataDobanda = altCont.rataDobanda;
    }

    // ── Proprietate ─────────────────────────────────────────
    public double RataDobanda
    {
        get { return rataDobanda; }
        set
        {
            if (value < 0) throw new ArgumentException("Rata dobanzii nu poate fi negativa!");
            rataDobanda = value;
        }
    }

    // ── OVERRIDE metoda virtuala (polimorfism) ───────────────
    public override void AfiseazaInfo()
    {
        Console.WriteLine("  [Cont Economii]");
        base.AfiseazaInfo();                        // afisam info din clasa de baza
        Console.WriteLine($"  Dobanda : {rataDobanda}% / an");
    }

    // ── Metoda specifica acestei clase ─────────────────────
    public void AplicaDobanda()
    {
        double dobanda = sold * rataDobanda / 100.0;
        sold += dobanda;
        tranzactii.Add(new Tranzactie("Dobanda", dobanda, $"{rataDobanda}% / an"));
        Console.WriteLine($"  Dobanda aplicata: +{dobanda:F2} RON. Sold nou: {sold:F2} RON");
    }

    // ── Conversie explicita ─────────────────────────────────
    public static explicit operator string(ContEconomii c)
        => $"ContEconomii,{c.titular},{c.sold},{c.rataDobanda}";

    public override string ToString()
        => $"[ContEconomii] {titular} | Sold: {sold:F2} RON | Dobanda: {rataDobanda}%";
}


// ════════════════════════════════════════════════════════════
//  CLASA DERIVATA (nivel 2) – ContCurent  : Cont
// ════════════════════════════════════════════════════════════
class ContCurent : Cont
{
    private double limitaDescoperit;   // cat poate intra pe minus

    // ── Constructor de instanta ─────────────────────────────
    public ContCurent(string titular, double soldInitial, double limitaDescoperit)
        : base(titular, soldInitial)
    {
        this.limitaDescoperit = limitaDescoperit;
    }

    // ── Constructor de copiere ──────────────────────────────
    public ContCurent(ContCurent altCont)
        : base(altCont)
    {
        this.limitaDescoperit = altCont.limitaDescoperit;
    }

    // ── Proprietate ─────────────────────────────────────────
    public double LimitaDescoperit
    {
        get { return limitaDescoperit; }
        set { limitaDescoperit = value; }
    }

    // ── OVERRIDE metoda virtuala (polimorfism) ───────────────
    public override void AfiseazaInfo()
    {
        Console.WriteLine("  [Cont Curent]");
        base.AfiseazaInfo();
        Console.WriteLine($"  Descoperit : pana la {limitaDescoperit:F2} RON");
    }

    // ── Metoda specifica acestei clase ─────────────────────
    public override bool Retrage(double suma)
    {
        if (sold - suma >= -limitaDescoperit)
        {
            sold -= suma;
            tranzactii.Add(new Tranzactie("Retragere", suma));
            Console.WriteLine($"  - Retras {suma:F2} RON. Sold nou: {sold:F2} RON");
            return true;
        }
        Console.WriteLine($"  ! Fonduri insuficiente. Sold: {sold:F2}, Limita: {limitaDescoperit:F2}");
        return false;
    }

    // ── Conversie explicita ─────────────────────────────────
    public static explicit operator string(ContCurent c)
        => $"ContCurent,{c.titular},{c.sold},{c.limitaDescoperit}";

    public override string ToString()
        => $"[ContCurent] {titular} | Sold: {sold:F2} RON | Descoperit: {limitaDescoperit:F2} RON";
}


// ════════════════════════════════════════════════════════════
//  PROGRAM PRINCIPAL
// ════════════════════════════════════════════════════════════
class Program
{
    // ── Fisiere ──────────────────────────────────────────────
    const string FISIER_DATE = "date.txt";
    const string RAPORT_ZERO = "sold_zero.txt";
    const string RAPORT_DOBANDA = "dobanda_mare.txt";

    static void Main()
    {
        Console.WriteLine("════════════════════════════════════════");
        Console.WriteLine("       SISTEM BANCAR - MyBank SRL       ");
        Console.WriteLine($"       Banca: {Cont.BANCA}");
        Console.WriteLine("════════════════════════════════════════\n");

        List<Cont> conturi = IncarcaDate(FISIER_DATE);

        bool running = true;
        while (running)
        {
            AfiseazaMeniu();
            string optiune = Console.ReadLine()?.Trim() ?? "";

            switch (optiune)
            {
                case "1": ListeazaConturi(conturi); break;
                case "2": CreazaCont(conturi); break;
                case "3": MeniuDepune(conturi); break;
                case "4": MeniuRetrage(conturi); break;
                case "5": MeniuDobanda(conturi); break;
                case "6": MeniuDetalii(conturi); break;
                case "7": GenereazaRaportSoldZero(conturi, RAPORT_ZERO); break;
                case "8": GenereazaRaportDobandaMare(conturi, RAPORT_DOBANDA); break;
                case "9": MeniuTransfer(conturi); break;
                case "10": MeniuIstoricTranzactii(conturi); break;
                case "0":
                    SalveazaDate(conturi, FISIER_DATE);
                    Console.WriteLine("La revedere!");
                    running = false;
                    break;
                default:
                    Console.WriteLine("\n  ! Optiune invalida.\n");
                    break;
            }
        }
    }

    static void AfiseazaMeniu()
    {
        Console.WriteLine("════════════════════════════════════════");
        Console.WriteLine("  MENIU PRINCIPAL");
        Console.WriteLine("════════════════════════════════════════");
        Console.WriteLine("  1. Listeaza toate conturile");
        Console.WriteLine("  2. Creeaza cont nou");
        Console.WriteLine("  3. Depune bani");
        Console.WriteLine("  4. Retrage bani");
        Console.WriteLine("  5. Aplica dobanda (ContEconomii)");
        Console.WriteLine("  6. Detalii cont");
        Console.WriteLine("  7. Raport: conturi cu sold 0");
        Console.WriteLine("  8. Raport: dobanda > 3%");
        Console.WriteLine("  9. Transfer intre conturi");
        Console.WriteLine(" 10. Istoric tranzactii");
        Console.WriteLine("  0. Salveaza si iesi");
        Console.WriteLine("════════════════════════════════════════");
        Console.Write("  Alegeti optiunea: ");
    }

    static void ListeazaConturi(List<Cont> conturi)
    {
        Console.WriteLine($"\n  Total conturi: {conturi.Count}\n");
        for (int i = 0; i < conturi.Count; i++)
            Console.WriteLine($"  [{i + 1}] {conturi[i]}");
        Console.WriteLine();
    }

    static Cont? SelecteazaCont(List<Cont> conturi)
    {
        ListeazaConturi(conturi);
        Console.Write("  Numarul contului: ");
        if (int.TryParse(Console.ReadLine(), out int idx) && idx >= 1 && idx <= conturi.Count)
            return conturi[idx - 1];
        Console.WriteLine("  ! Selectie invalida.\n");
        return null;
    }

    static void CreazaCont(List<Cont> conturi)
    {
        Console.WriteLine("\n  Tip cont: 1=Cont  2=ContCurent  3=ContEconomii");
        Console.Write("  Alegeti: ");
        string tip = Console.ReadLine()?.Trim() ?? "";

        Console.Write("  Titular: ");
        string titular = Console.ReadLine()?.Trim() ?? "";
        if (string.IsNullOrEmpty(titular)) { Console.WriteLine("  ! Titular invalid.\n"); return; }

        Console.Write("  Sold initial (RON): ");
        if (!double.TryParse(Console.ReadLine(), out double sold) || sold < 0)
        {
            Console.WriteLine("  ! Sold invalid.\n");
            return;
        }

        switch (tip)
        {
            case "1":
                conturi.Add(new Cont(titular, sold));
                break;
            case "2":
                Console.Write("  Limita descoperit (RON): ");
                if (!double.TryParse(Console.ReadLine(), out double limita) || limita < 0)
                {
                    Console.WriteLine("  ! Limita invalida.\n"); return;
                }
                conturi.Add(new ContCurent(titular, sold, limita));
                break;
            case "3":
                Console.Write("  Rata dobanda (%): ");
                if (!double.TryParse(Console.ReadLine(), out double dobanda) || dobanda < 0)
                {
                    Console.WriteLine("  ! Dobanda invalida.\n"); return;
                }
                conturi.Add(new ContEconomii(titular, sold, dobanda));
                break;
            default:
                Console.WriteLine("  ! Tip invalid.\n");
                return;
        }

        Console.WriteLine($"  Cont creat. Total conturi: {Cont.GetTotalConturi()}\n");
    }

    static void MeniuDepune(List<Cont> conturi)
    {
        Console.WriteLine("\n  === DEPUNERE ===");
        Cont? cont = SelecteazaCont(conturi);
        if (cont == null) return;

        Console.Write("  Suma (RON): ");
        if (!double.TryParse(Console.ReadLine(), out double suma) || suma <= 0)
        {
            Console.WriteLine("  ! Suma invalida.\n"); return;
        }

        Console.Write("  Descriere (Enter pentru a sari): ");
        string desc = Console.ReadLine()?.Trim() ?? "";

        if (string.IsNullOrEmpty(desc))
            cont.Depune(suma);
        else
            cont.Depune(suma, desc);
        Console.WriteLine();
    }

    static void MeniuRetrage(List<Cont> conturi)
    {
        Console.WriteLine("\n  === RETRAGERE ===");
        Cont? cont = SelecteazaCont(conturi);
        if (cont == null) return;

        Console.Write("  Suma (RON): ");
        if (!double.TryParse(Console.ReadLine(), out double suma) || suma <= 0)
        {
            Console.WriteLine("  ! Suma invalida.\n"); return;
        }

        cont.Retrage(suma);
        Console.WriteLine();
    }

    static void MeniuTransfer(List<Cont> conturi)
    {
        Console.WriteLine("\n  === TRANSFER ===");
        Console.WriteLine("  Selectati contul SURSA:");
        Cont? sursa = SelecteazaCont(conturi);
        if (sursa == null) return;

        Console.WriteLine("  Selectati contul DESTINATIE:");
        Cont? dest = SelecteazaCont(conturi);
        if (dest == null) return;

        if (ReferenceEquals(sursa, dest))
        {
            Console.WriteLine("  ! Sursa si destinatia nu pot fi acelasi cont.\n"); return;
        }

        Console.Write("  Suma (RON): ");
        if (!double.TryParse(Console.ReadLine(), out double suma) || suma <= 0)
        {
            Console.WriteLine("  ! Suma invalida.\n"); return;
        }

        if (sursa.Retrage(suma))
            dest.Depune(suma, $"transfer de la {sursa.IBAN}");
        Console.WriteLine();
    }

    static void MeniuIstoricTranzactii(List<Cont> conturi)
    {
        Console.WriteLine("\n  === ISTORIC TRANZACTII ===");
        Cont? cont = SelecteazaCont(conturi);
        if (cont == null) return;

        Console.WriteLine($"\n  Cont: {cont}");
        Console.WriteLine($"  {new string('-', 55)}");

        if (cont.Tranzactii.Count == 0)
        {
            Console.WriteLine("  (nicio tranzactie inregistrata)");
        }
        else
        {
            foreach (Tranzactie t in cont.Tranzactii)
                Console.WriteLine(t);
        }

        Console.WriteLine($"  {new string('-', 55)}\n");
    }

    static void MeniuDobanda(List<Cont> conturi)
    {
        Console.WriteLine("\n  === APLICA DOBANDA ===");
        Cont? cont = SelecteazaCont(conturi);
        if (cont == null) return;

        if (cont is not ContEconomii ce)
        {
            Console.WriteLine("  ! Dobanda se aplica doar pe ContEconomii.\n"); return;
        }

        ce.AplicaDobanda();
        Console.WriteLine();
    }

    static void MeniuDetalii(List<Cont> conturi)
    {
        Console.WriteLine("\n  === DETALII CONT ===");
        Cont? cont = SelecteazaCont(conturi);
        if (cont == null) return;

        Console.WriteLine();
        cont.AfiseazaInfo();
        Console.WriteLine();
    }

    static void SalveazaDate(List<Cont> conturi, string numeFisier)
    {
        using StreamWriter sw = new StreamWriter(numeFisier);
        foreach (Cont c in conturi)
        {
            string linie = c switch
            {
                ContEconomii ce => (string)ce,
                ContCurent cc  => (string)cc,
                _              => (string)c
            };
            sw.WriteLine(linie);
        }
        Console.WriteLine($"\n  Date salvate in '{numeFisier}'.\n");
    }


    // ════════════════════════════════════════════════════════
    //  Incarca date din fisier (sau creeaza date exemplu)
    // ════════════════════════════════════════════════════════
    static List<Cont> IncarcaDate(string numeFisier)
    {
        // Daca fisierul nu exista, il cream cu date exemplu
        if (!File.Exists(numeFisier))
        {
            Console.WriteLine($"Fisierul '{numeFisier}' nu exista. Il cream cu date exemplu...");
            string[] liniiExemplu =
            {
                "Cont,Ion Popescu,0",
                "ContCurent,Maria Ionescu,1500,2000",
                "ContEconomii,Andrei Popa,5000,4.5",
                "ContCurent,Elena Dumitrescu,200,500",
                "ContEconomii,Radu Gheorghe,0,2.0",
                "ContEconomii,Ana Stanescu,8000,5.0"
            };
            File.WriteAllLines(numeFisier, liniiExemplu);
            Console.WriteLine($"  Fisier '{numeFisier}' creat cu {liniiExemplu.Length} inregistrari.\n");
        }

        // Citim fisierul linie cu linie
        List<Cont> lista = new List<Cont>();
        string[] linii = File.ReadAllLines(numeFisier);

        Console.WriteLine($"Se incarca date din '{numeFisier}'...");

        foreach (string linie in linii)
        {
            if (string.IsNullOrWhiteSpace(linie)) continue;

            string[] parti = linie.Split(',');
            string tip = parti[0].Trim();

            // Cream obiectul potrivit tipului din fisier
            if (tip == "ContEconomii")
            {
                string titular = parti[1].Trim();
                double sold = double.Parse(parti[2].Trim());
                double dobanda = double.Parse(parti[3].Trim());
                lista.Add(new ContEconomii(titular, sold, dobanda));
            }
            else if (tip == "ContCurent")
            {
                string titular = parti[1].Trim();
                double sold = double.Parse(parti[2].Trim());
                double limita = double.Parse(parti[3].Trim());
                lista.Add(new ContCurent(titular, sold, limita));
            }
            else  // "Cont" de baza
            {
                string titular = parti[1].Trim();
                double sold = double.Parse(parti[2].Trim());
                lista.Add(new Cont(titular, sold));
            }
        }

        Console.WriteLine($"  Incarcat {lista.Count} conturi.\n");
        return lista;
    }


    // ════════════════════════════════════════════════════════
    //  RAPORT 1: Conturi cu sold = 0
    // ════════════════════════════════════════════════════════
    static void GenereazaRaportSoldZero(List<Cont> conturi, string numeFisier)
    {
        Console.WriteLine($"════ RAPORT 1: Conturi cu sold 0 → '{numeFisier}' ════\n");

        using StreamWriter sw = new StreamWriter(numeFisier);
        sw.WriteLine("RAPORT: Conturi cu sold 0");
        sw.WriteLine($"Generat la: {DateTime.Now}");
        sw.WriteLine(new string('-', 40));

        int nr = 0;
        foreach (Cont c in conturi)
        {
            if (c.Sold == 0)
            {
                sw.WriteLine(c.ToString());
                Console.WriteLine($"  > {c}");
                nr++;
            }
        }

        sw.WriteLine(new string('-', 40));
        sw.WriteLine($"Total: {nr} conturi cu sold 0.");

        Console.WriteLine($"  {nr} conturi scrise in '{numeFisier}'\n");
    }


    // ════════════════════════════════════════════════════════
    //  RAPORT 2: ContEconomii cu dobanda > 3%
    // ════════════════════════════════════════════════════════
    static void GenereazaRaportDobandaMare(List<Cont> conturi, string numeFisier)
    {
        Console.WriteLine($"════ RAPORT 2: Dobanda > 3% → '{numeFisier}' ════\n");

        using StreamWriter sw = new StreamWriter(numeFisier);
        sw.WriteLine("RAPORT: Conturi Economii cu dobanda > 3%");
        sw.WriteLine($"Generat la: {DateTime.Now}");
        sw.WriteLine(new string('-', 40));

        int nr = 0;
        foreach (Cont c in conturi)
        {
            // 'is' verifica tipul si face cast simultan
            if (c is ContEconomii ce && ce.RataDobanda > 3.0)
            {
                sw.WriteLine(ce.ToString());
                Console.WriteLine($"  > {ce}");
                nr++;
            }
        }

        sw.WriteLine(new string('-', 40));
        sw.WriteLine($"Total: {nr} conturi cu dobanda > 3%.");

        Console.WriteLine($"  {nr} conturi scrise in '{numeFisier}'\n");
    }
}
