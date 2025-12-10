using System;
using System.Diagnostics;

//Files and Generator 
string DataFile = "patients.txt";
string ReportLogFile = "PatientReportLog.txt";
Random idString = new Random();

//formating for Patients and testing
List<Patient> patients = new List<Patient>();
LoadPatients(ref patients, DataFile);
{
    Patient test = new Patient
    {
        Id = 999999,
        Name = "Test",
        Notes = "Some notes",
        Insurance = "TestIns",
        Medications = new List<string> { "MedA", "MedB" },
        Allergies = new List<string> { "All1", "All2" },
        LastEdit = "2000-01-01 00:00:00"
    };

    List<string> lines = test.ToFileLines();
    Debug.Assert(lines.Count > 0, "ToFileLines returned empty.");

    int id = GenerateUniqueID(new List<Patient> { test });
    Debug.Assert(id != 999999, "Unique ID failed.");
}

//User Input Menu
bool running = true;
while (running)
{
    Console.Clear();
    Console.WriteLine("+=+=+ Patient Charting System +=+=+");
    Console.WriteLine("1) List Patients");
    Console.WriteLine("2) Add Patient");
    Console.WriteLine("3) Save & Exit");
    Console.Write("Choice: ");

    string choice = Console.ReadLine();
    switch (choice)
    {
        case "1": ListPatient(patients); break;
        case "2": AddPatient(patients); break;
        case "3":
            SavePatients(patients, DataFile);
            running = false;
            break;
        default: 
            Console.WriteLine("Invalid Choice");
            break;
    }

    if (running)
    {
        Console.WriteLine("\nPress Enter to continue");
        while (Console.ReadKey(true).Key != ConsoleKey.Enter) ;
    }
}

//Pulls patients from the files and displays them
void LoadPatients(ref List<Patient> patients, string filePath)
{
    if (!File.Exists(filePath))
        File.WriteAllText(filePath, "");

    string[] lines = File.ReadAllLines(filePath);
    patients = new List<Patient>();

    int index = 0;
    while (index < lines.Length)
    {
        Patient p = Patient.FromFileLines(lines, ref index);
        if (p != null)
            patients.Add(p);
    }
}

//given the inputs, saves the newer patients I have
void SavePatients(List<Patient> patients, string filePath)
{
    List<string> outLine = new List<string>();
    foreach (var p in patients)
    {
        List<string> block = p.ToFileLines();
        foreach (var line in block)
            outLine.Add(line);
    }
    File.WriteAllLines(filePath, outLine);
}

//Displays the patients
void ListPatient(List<Patient> patients)
{
    Console.WriteLine("\nPatients:\n");

    if (patients.Count == 0)
    {
        Console.WriteLine("No patients.");
        return;
    }

    foreach (var p in patients)
        Console.WriteLine($"{p.Id} | {p.Name} | {p.Insurance} | Updated: {p.LastEdit}");

    //Pulls patient with use of the correct Id
    Console.Write("\nEnter ID to view details, blank to exit: ");
    string openDetails = Console.ReadLine();

    if (int.TryParse(openDetails.Trim(), out int id))
    {
        var p = patients.Find(x => x.Id == id);
        if (p == null)
        {
            Console.WriteLine("Patient not found.");
            return;
        }

        Console.WriteLine($"\n{p.Name} (ID {p.Id})");
        Console.WriteLine($"Notes: {p.Notes}");
        Console.WriteLine($"Insurance: {p.Insurance}");
        Console.WriteLine($"Last Updated: {p.LastEdit}");

        Console.WriteLine("\nMedications:");
        if (p.Medications.Count == 0) Console.WriteLine("  (none)");
        else 
        {
            foreach (var m in p.Medications) 
                Console.WriteLine(" - " + m);
        }

        Console.WriteLine("\nAllergies:");
        if (p.Allergies.Count == 0) Console.WriteLine("  (none)");
        else 
        {
            foreach (var a in p.Allergies) 
                Console.WriteLine(" - " + a);
        }
    }
}

//When I want to add a patient, it'll ask for details to put in for recording
void AddPatient(List<Patient> patients)
{
    Console.WriteLine("\nAdd Patient");

    Patient p = new Patient();
    p.Id = GenerateUniqueID(patients);

    Console.Write("Name: ");
    p.Name = Console.ReadLine();

    Console.Write("Notes: ");
    p.Notes = Console.ReadLine();

    Console.Write("Insurance: ");
    p.Insurance = Console.ReadLine();

    Console.WriteLine("Medications (type 'done' when finished):");
    string medicate;
    do
    {
        //done to end program
        medicate = Console.ReadLine();
        if (medicate.Trim().ToLower() != "done" && medicate != "")
            p.Medications.Add(medicate);
    }
    while (medicate.Trim().ToLower() != "done");

    Console.WriteLine("Allergies (type 'done' when finished):");
    string allergic;
    do
    {
        //done to end program
        allergic = Console.ReadLine();
        if (allergic.Trim().ToLower() != "done" && allergic != "")
            p.Allergies.Add(allergic);
    }
    while (allergic.Trim().ToLower() != "done");

    //Adds patient to the file with the time and date they were submitted
    p.LastEdit = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    patients.Add(p);

    Console.WriteLine($" {p.Name} (ID {p.Id})");
    Console.WriteLine($"Added new patient {p.Name} with ID {p.Id}");
}

//Reports everything entered into the file
void WriteReport(string message)
{
    string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
    File.AppendAllText(ReportLogFile, line + "\r\n");
}

//Makes the ID
int GenerateUniqueID(List<Patient> patients)
{
    List<int> existingIds = patients.Select(x => x.Id).ToList();
    int id;

    do
    {
        id = idString.Next(100000, 999999); 
    } 
    while (existingIds.Contains(id)); 

    return id;
}

//How I classed the need for the patients
class Patient
{
    public int Id;
    public string Name;
    public string Notes;
    public string Insurance;
    public List<string> Medications = new List<string>();
    public List<string> Allergies = new List<string>();
    public string LastEdit;

    public List<string> ToFileLines()
    {
        List<string> lines = new List<string>();
        lines.Add($"{Id}|{Escape(Name)}|{Escape(Notes)}|{Escape(Insurance)}|{LastEdit}");

        lines.Add("MEDICATIONS");
        if (Medications.Count == 0) lines.Add("");
        else foreach (var meds in Medications) lines.Add(Escape(meds));

        lines.Add("ALLERGIES");
        if (Allergies.Count == 0) lines.Add("");
        else foreach (var a in Allergies) lines.Add(Escape(a));

        lines.Add("End");
        return lines;
    }

//Formatting in the PatientReportLog
public static Patient FromFileLines(string[] lines, ref int index)
{
    while (index < lines.Length && lines[index].Trim().Length == 0)
        index++;

    if (index >= lines.Length)
        return null; 

    string head = lines[index].Trim();
    string[] parts = head.Split('|');

    if (parts.Length < 5)
    {
        index++;
        return null;
    }

    Patient p = new Patient
    {
        Id = int.Parse(parts[0]),
        Name = parts[1],
        Notes = parts[2],
        Insurance = parts[3],
        LastEdit = parts[4]
    };
    index++;

    if (index < lines.Length && lines[index].Trim() == "MEDICATIONS")
    {
        index++;
        while (index < lines.Length && lines[index].Trim() != "ALLERGIES")
        {
            string med = lines[index].Trim();
            if(med.Length > 2 && !med.Contains("?"));
                {
                    p.Medications.Add(med);
                }
            index++;
        }
    }

    if (index < lines.Length && lines[index].Trim() == "ALLERGIES")
    {
        index++;
        while (index < lines.Length && lines[index].Trim() != "End")
        {
            string allergen = lines[index].Trim();
            if(allergen.Length > 2 && !allergen.Contains("?"));
                {
                    p.Allergies.Add(allergen);
                }
            index++;
        }
    }
    //When at the end of the descriptors in PatientReportLog, cuts off by giving end
    if (index < lines.Length && lines[index].Trim() == "End")
        index++;

    return p;
}

    private string Escape(string s)
    {
        if (s == null) return "";
        return s.Replace("\r", "").Replace("\n", "").Replace("|", ",");
    }
}
