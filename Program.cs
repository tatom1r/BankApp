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
        Console.WriteLine($"  + Depus {suma:F2} RON. Sold nou: {sold:F2} RON");
    }

    public void Depune(double suma, string descriere)
    {
        sold += suma;
        Console.WriteLine($"  + Depus {suma:F2} RON ({descriere}). Sold nou: {sold:F2} RON");
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
        Console.WriteLine($"  Dobanda aplicata: +{dobanda:F2} RON. Sold nou: {sold:F2} RON");
    }

    // ── Conversie explicita ─────────────────────────────────
    public static new explicit operator string(ContEconomii c)
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
    public bool Retrage(double suma)
    {
        if (sold - suma >= -limitaDescoperit)
        {
            sold -= suma;
            Console.WriteLine($"  - Retras {suma:F2} RON. Sold nou: {sold:F2} RON");
            return true;
        }
        Console.WriteLine($"  ! Fonduri insuficiente. Sold: {sold:F2}, Limita: {limitaDescoperit:F2}");
        return false;
    }

    // ── Conversie explicita ─────────────────────────────────
    public static new explicit operator string(ContCurent c)
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
        Console.WriteLine($"       Banca: {Cont.BANCA}            ");
        Console.WriteLine("════════════════════════════════════════\n");

        // ── 1. Restauram datele din fisier (sau cream fisierul) ──
        List<Cont> conturi = IncarcaDate(FISIER_DATE);

        Console.WriteLine($"\nTotal conturi create pana acum: {Cont.GetTotalConturi()}\n");

        // ── 2. Demonstram polimorfismul ──────────────────────────
        Console.WriteLine("════ INFORMATII CONTURI (polimorfism) ══════\n");
        foreach (Cont c in conturi)
        {
            c.AfiseazaInfo();   // apelul merge la metoda corecta (ContEconomii sau ContCurent)
            Console.WriteLine();
        }

        // ── 3. Demonstram supraincarcarea ────────────────────────
        Console.WriteLine("════ TRANZACTII (supraincarcari) ═══════════\n");
        conturi[0].Depune(200);
        conturi[0].Depune(500, "salariu");

        if (conturi[1] is ContCurent cc)
            cc.Retrage(3000);

        if (conturi[2] is ContEconomii ce)
            ce.AplicaDobanda();

        // ── 4. Constructorul de copiere ──────────────────────────
        Console.WriteLine("\n════ CONSTRUCTOR DE COPIERE ════════════════\n");
        ContEconomii original = (ContEconomii)conturi[2];
        ContEconomii copie = new ContEconomii(original);
        Console.WriteLine($"  Original : {original}");
        Console.WriteLine($"  Copie    : {copie}");
        Console.WriteLine("  (IBAN diferit – fiecare cont e unic!)\n");

        // ── 5. Conversie explicita ───────────────────────────────
        Console.WriteLine("════ CONVERSIE EXPLICITA (Cont → string) ═══\n");
        string linie = (string)(ContEconomii)conturi[2];
        Console.WriteLine($"  Linie CSV: {linie}\n");

        // ── 6. Raport 1: conturi cu sold 0 ──────────────────────
        GenereazaRaportSoldZero(conturi, RAPORT_ZERO);

        // ── 7. Raport 2: conturi economii cu dobanda > 3% ────────
        GenereazaRaportDobandaMare(conturi, RAPORT_DOBANDA);

        Console.WriteLine("\n════ GATA! ══════════════════════════════════");
        Console.WriteLine($"  Fisiere generate: {RAPORT_ZERO}, {RAPORT_DOBANDA}");
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
