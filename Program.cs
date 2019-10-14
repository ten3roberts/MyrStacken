using System;
using System.Collections.Generic;
using System.IO;

namespace MyrStacken
{
    class Program
    {
        const string header = "--- Myrstacken ---";
        string[] all_names;
        //Indicates if name at pos in names is occupied
        List<string> available_names;
        public static string Input(string msg)
        {
            Console.Write(msg + " > ");
            return Console.ReadLine();
        }
        public static string Title(string str)
        {
            if (str == "" || str == null) return "";
            char[] result = str.ToLower().ToCharArray();
            result[0] = char.ToUpper(result[0]);
            return new string(result);
        }
        public static int Num(string str)
        {
            int tmp;
            if (!int.TryParse(str, out tmp) && str.Length > 0) Console.WriteLine("Invalid number '" + str + "'");
            return tmp;
        }

        //A wrapper for the tryGetValue function to give a return value instead of out argument and un-nullify result
        public string GetValue(Dictionary<string, string> dictionary, string key)
        {
            string tmp;
            if (!dictionary.TryGetValue(key, out tmp)) return "";

            return tmp;
        }
        //A flag indicating if the program should exit
        bool should_exit;
        //A list containing all the ants in the 'hill
        List<Ant> ants;
        Program()
        {
            //Loading file Names.txt and loading all the names to be used by the Generate function
            all_names = File.ReadAllLines("./Names.txt");
            available_names = new List<string>(all_names);

            should_exit = false;
            ants = new List<Ant>();
            Run();
        }

        static void Main(string[] args)
        {
            Program program = new Program();
        }

        void Run()
        {
            Console.WriteLine(header);

            while (!should_exit)
            {
                InputHandler();
            }
        }

        //Handles commands
        void InputHandler()
        {
            //The different parts of the command separated by spaces. The fisrt one is the command itself, the others are aguments
            string[] parts = Input("\nEnter command >").ToLower().Split(' ');

            Dictionary<string, string> args = new Dictionary<string, string>();
            //Making a dictionary of the flags and params, skipping command itself
            for (int i = 1; i < parts.Length; i++)
            {
                //If current part is a flag
                //Checking for empty arg if there is a space at the end of the command
                if (parts[i] != "" && parts[i][0] == '-')
                {
                    //And next part is not a flag; associate flag with param
                    if (i < parts.Length - 1 && parts[i + 1][0] != '-')
                        args[parts[i]] = parts[i + 1];
                    //If only flag is present, it is set to empty but existing
                    else
                        args[parts[i]] = "";
                }
            }

            switch (parts[0])
            {
                case "add":
                    Add(parts);
                    break;
                case "remove":
                case "rm":
                    Remove(args);
                    break;
                case "list":
                case "ls":
                    ListAnts(args);
                    break;
                case "find":
                    Find(args);
                    break;
                case "generate":
                case "gen":
                    Generate(args);
                    break;
                case "clear":
                    Console.Clear();
                    Console.WriteLine(header);
                    break;
                case "help":
                    Console.WriteLine(
                        "add : Usage \"add {name} {numLegs}\"\n\tadds an ant with name and number of legs\n" +
                        "remove/rm : Usage \"remove {flag} {param} ...\"\n\tRemoves an ant with given parameters. Removes none if no arguments are given.\n\t-n : by name\n\t-l : by number of legs\n\t-a : removes all\n" +
                        "list/ls : Usage \"list {flag} {param} ...\"\n\tLists all ants in the colony. Displays ants in order of insertion if no arguments are supplied\n\t-l : show ants with matching amount of legs\n\t-sl : sorts by amount of legs\n\t-sn : sorts by name\n" +
                        "find : Usage \"find {flag} {param} ...\"\n\tFinds ant by specified parameters. Displays all if no arguments are given.\n\t-n : Displays the ant with matching name\n\t-l : Displays all ants with matching amount of legs\n" +
                        "generate/gen : Usage {flag} {param} ...\n\tGenerates random ants\n\t-c : Amount of ants to generate. Default 1\n\t-minl : Minimum amount of legs per ant. Default : 1\n\t-maxl : Maximum of legs per ant. Default 10\n" +
                        "exit : Closes the program\n" +
                        "Several flags can be used at the same time and are not order dependent"
                        );
                    break;
                case "exit":
                    should_exit = true;
                    break;
                default:
                    Console.WriteLine("Unknown command, use 'help' for available commands");
                    break;
            }
        }

        void Add(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Incorrect usage of add, type 'help' for usage");
                return;
            }
            string name = Title(args[1]);
            int num_legs = Num(args[2]);

            foreach (Ant ant in ants)
            {
                if (ant.Name == name)
                {
                    Console.WriteLine("Ant with name " + name + " already exists");
                    return;
                }
            }
            if (name.Length == 0)
            {
                Console.WriteLine("Ant's name can't be empty");
                return;
            }
            if (name.Length > 10)
            {
                Console.WriteLine("Ant's name can't be more than 10 characters long");
                return;
            }
            if (num_legs < 0)
            {
                Console.WriteLine("Ant's number of legs can't be negative");
                return;
            }
            ants.Add(new Ant(name, num_legs));
            Console.WriteLine("Adding ant " + name + " with " + num_legs + " legs");
            //Removes name from available_names if it exists to avoid generate command to generate duplicates
            for (int i = 0; i < available_names.Count; i++) if (available_names[i] == name) available_names.RemoveAt(i);

        }
        void Remove(Dictionary<string, string> args)
        {
            string name_arg = Title(GetValue(args, "-n"));
            int leg_arg = Num(GetValue(args, "-l"));
            bool all = args.ContainsKey("-a");
            bool verbose = !args.ContainsKey("-m");

            for (int i = 0; i < ants.Count; i++)
            {
                bool match = name_arg.Length > 0 || leg_arg != 0 || all;
                if (!match)
                {
                    Console.WriteLine("No arguments for deletion specified; removing none. Use '-a' to remove all ants");
                    return;
                }
                if (name_arg != "")
                    match = (ants[i].Name == name_arg) && match;
                if (leg_arg != 0)
                    match = (ants[i].NumLegs == leg_arg) && match;
                if (match)
                {
                    if (verbose)
                        Console.WriteLine("Removing ant " + ants[i].Name);

                    //If the ant to be removed's name was in the available_names, add it back as available so it can be picked again by gen
                    for (int j = 0; j < all_names.Length; j++) if (all_names[j] == ants[i].Name) available_names.Add(ants[i].Name);

                    ants.RemoveAt(i);

                    i--;
                }
            }
        }

        List<Ant> Find(Dictionary<string, string> args)
        {
            List<Ant> result = new List<Ant>();

            string name_arg = "";
            int leg_arg = -1;
            bool verbose = !args.ContainsKey("-m");
            if (args.ContainsKey("-l")) leg_arg = Num(args["-l"]);
            if (args.ContainsKey("-n")) name_arg = Title(args["-n"]);

            foreach (Ant a in ants)
            {
                bool match = true;
                if (name_arg != "")
                    match = (a.Name == name_arg) && match;
                if (leg_arg != -1)
                    match = (a.NumLegs == leg_arg) && match;
                if (match)
                {
                    result.Add(a);
                    if (verbose) Console.WriteLine("[" + (result.Count - 1) + "]: " + a.Name + ", " + a.NumLegs);
                }
            }

            if (verbose) Console.WriteLine("Found " + result.Count + " ants");
            return result;
        }

        void ListAnts(Dictionary<string, string> args)
        {
            //0 : Nothing, 1 : By name, 2 : By legs
            int compare = (args.ContainsKey("-sn") ? 1 : args.ContainsKey("-sl") ? 2 : 0);
            args["-m"] = "";

            List<Ant> result = Find(args);

            IComparer<Ant> comparer = null;
            if (compare == 1)
                comparer = new CompareNames();
            else if (compare == 2)
                comparer = new CompareLegs();

            if (compare != 0)
                result.Sort(comparer);
            for (int i = 0; i < result.Count; i++) { Console.WriteLine("[" + i + "]: " + result[i].Name + ", " + result[i].NumLegs); }
            Console.WriteLine("Ants listed: " + result.Count + "/" + ants.Count);
        }

        void Generate(Dictionary<string, string> args)
        {
            Random random = new Random();
            int amount = 1;
            int min_legs = 1;
            int max_legs = 10;

            if (args.ContainsKey("-c")) amount = Num(args["-c"]);
            if (args.ContainsKey("-minl")) min_legs = Num(args["-minl"]);
            if (args.ContainsKey("-maxl")) max_legs = Num(args["-maxl"]);

            for (int i = 0; i < amount; i++)
            {
                //There are no more available names from the list loaded from file
                if (available_names.Count == 0)
                {
                    Console.WriteLine("No more available names to choose. Stopping command");
                    return;
                }

                //Picks a random name from available names list
                int name_id = random.Next(0, available_names.Count);
                string name = available_names[name_id];
                int numLegs = random.Next(min_legs, max_legs);

                //Uses the add command to add the new ant
                Add(new string[] { "add", name, numLegs.ToString() });
            }
        }
    }

    public class CompareNames : IComparer<Ant>
    { public int Compare(Ant a, Ant b) { return string.Compare(a.Name, b.Name); } }

    public class CompareLegs : IComparer<Ant>
    { public int Compare(Ant a, Ant b) { return (a.NumLegs >= b.NumLegs ? 1 : -1); } }

    public class Ant
    {
        string name;
        int numLegs;
        public string Name
        {
            get { return name; }
            set
            {
                if (value.Length == 0)
                {
                    Console.WriteLine("Ant's name can't be empty");
                    return;
                }
                if (value.Length >= 10)
                {
                    Console.WriteLine("Ant's name can't be more than 10 characters long");
                    return;
                }
                name = value;
            }
        }
        public int NumLegs
        {
            get { return numLegs; }
            set
            {
                if (value < 0)
                {
                    Console.WriteLine("Ant's number of legs can't be negative");
                    return;
                }
                numLegs = value;
            }
        }

        public Ant() { name = "unnamed"; numLegs = 4; }
        public Ant(string name, int numLegs) { this.Name = Program.Title(name); NumLegs = numLegs; }
    }
}
