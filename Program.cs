using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Program
{
    public static void Main(string[] args)
    {
        List<Item> inv = new List<Item>();

        //Items Consumables
        Item hPotion = new Item("Health Potion", "hp", 40);
        Item mPotion = new Item("Mana Potion", "mp", 16);
        Item atkCandy = new Item("Attack Boost Candy", "atk", 1.3);
        Item defCandy = new Item("Defense Boost Candy", "def", 1.3);
        Item extraDefCandy = new Item("Candy of Iron Skin", "def", 1.6);
        inv.Add(new Item("Health Potion", "hp", 40));
        inv.Add(new Item("Health Potion", "hp", 40));
        inv.Add(atkCandy);
        inv.Add(defCandy);

        Fighter player = new Fighter("Hero", 100, 36, 16, 14, 20, 10, "fireball", "greater fireball", "heal", "agility");
        Fighter player2 = new Fighter("Lesser Hero", 20, 36, 16, 14, 10, 10, "fireball");
        Fighter theEnemy = new Fighter("Lesser Demon", 60, 10, 10, 16, 10, 11, "fireball", "agility");
        Fighter theEnemy2 = new Fighter("Bigger Demon", 80, 12, 12, 16, 13, 15, "fireball", "heal");

        //Items Equipment
        Item bSword = new Item("Bronze Sword", "atk", 6);
        Item iSword = new Item("Iron Sword", "atk", 14);

        //Add Equipment
        player.AddEquipment(iSword);

        Fighter[] players = new Fighter[] { player, player2 };
        Fighter[] enemies = new Fighter[] { theEnemy, theEnemy2 };

        //Zum Sortieren; Initialisieren des Equipments
        int index = 0;
        foreach (Fighter f in players)
        {
            f.isPlayer = true;
            f.origIndex = index;
            f.EvalEquipment();
            index++;
        }
        index = 0;
        foreach (Fighter f in enemies)
        {
            f.isPlayer = false;
            f.origIndex = index;
            index++;
        }

        //Zum Sortieren
        Fighter[] fightersSorted = new Fighter[players.Length + enemies.Length];
        FSort[] fsorts = new FSort[players.Length + enemies.Length];
        Array.Copy(players, fightersSorted, players.Length);
        Array.Copy(enemies, 0, fightersSorted, players.Length, enemies.Length);

        //Für BattleMsg, da ich derzeit nicht plane die Zugreihenfolge wie bei Octopath zu spoilern
        Fighter[] forBattleMsg = new Fighter[players.Length + enemies.Length];
        Array.Copy(fightersSorted, forBattleMsg, fightersSorted.Length);

        //Initialisiere Statuswerte
        Battle.InitiateStats(players);
        Battle.InitiateStats(enemies);

        string tmp;
        while (true)
        {
            fightersSorted = fightersSorted.OrderByDescending(o => o.tempSpd).ToArray();
            index = 0;
            foreach (Fighter f in fightersSorted)
            {
                fsorts[index] = new FSort(f.isPlayer, f.origIndex);
                index++;
            }

            StringBuilder choices = new StringBuilder();
            foreach (Fighter f in players)
            {
                Battle.BattleMsg(choices, players.Length, forBattleMsg);
                tmp = Battle.PlayerChoice(f, enemies, inv);
                if (tmp != "")
                    choices.AppendLine(tmp);
            }
            foreach (Fighter f in enemies)
            {
                Battle.EnemyChoice(f, players);
            }

            Console.Clear();

            foreach (FSort fs in fsorts)
            {
                int ind = fs.index;
                if (fs.isPlayer)
                {
                    int foeInd = players[ind].foeIndex;
                    if (foeInd != -1)
                    {
                        Battle.Turn(players[ind], enemies[foeInd], players, enemies, inv);
                    }
                    else
                        Battle.Turn(players[ind], enemies[0], players, enemies, inv, true);
                }
                else
                {
                    int foeInd = enemies[ind].foeIndex;
                    if (foeInd != -1)
                        Battle.Turn(enemies[ind], players[foeInd], players, enemies, inv);
                    else
                        Battle.Turn(enemies[ind], players[0], players, enemies, inv, true);
                }
            }

            bool done = true;
            do
            {
                done = true;
                int toRemove = -1;
                foreach (Item it in inv)
                {
                    toRemove++;
                    if (it.reserved == true)
                    {
                        done = false;
                        break;
                    }
                }
                if (done == false)
                    inv.RemoveAt(toRemove);
            } while (done != true);

            Console.WriteLine("Press any key to continue");
            Console.ReadLine();
        }
    }
}

public class FSort
{
    public bool isPlayer;
    public int index;

    public FSort(bool IsPlayer, int Index)
    {
        isPlayer = IsPlayer;
        index = Index;
    }
}

public class Fighter
{
    //Attacken- & Gegnerwahl
    public int choice;
    public int magiChoice;
    public int itemChoice;
    public int foeIndex;

    public bool isPlayer;
    public int origIndex;

    //Statuswerte
    public string name;
    public int maxHp;
    public int maxMp;
    public int atk;
    public int def;
    public int spd;
    public int mdef;

    public int hp;
    public int mp;
    public double tempAtk;
    public double tempDef;
    public double tempSpd;
    public double tempMdef;
    public readonly string[] skillset;
    public List<Item> equipment = new List<Item>();
    //public Inventory inv = new Inventory();

    public Fighter(string cName, int mHp, int cMp, int cAtk, int cDef, int cSpd, int cMdef, params string[] skills)
    {
        name = cName;
        maxHp = mHp;
        maxMp = cMp;
        atk = cAtk;
        def = cDef;
        spd = cSpd;
        mdef = cMdef;

        skillset = new string[skills.Length];
        var i = 0;
        foreach (string skill in skills)
        {
            skillset[i] = skill;
            i++;
        }
    }

    public void AddEquipment(params Item[] items)
    {
        foreach (Item it in items)
        {
            equipment.Add(it);
        }
    }

    public void EvalEquipment()
    {
        foreach (Item it in equipment)
        {
            switch (it.type)
            {
                case "hp":
                    maxHp += Convert.ToInt32(it.valnum);
                    break;
                case "mp":
                    maxMp += Convert.ToInt32(it.valnum);
                    break;
                case "atk":
                    atk += Convert.ToInt32(it.valnum);
                    break;
                case "def":
                    def += Convert.ToInt32(it.valnum);
                    break;
                case "mdef":
                    mdef += Convert.ToInt32(it.valnum);
                    break;
                case "spd":
                    spd += Convert.ToInt32(it.valnum);
                    break;

            }
        }
    }
}

public static class Skills
{
    private static readonly string fail = "... But it failed!";

    private static readonly Dictionary<string, int> skillcosts = new Dictionary<string, int>
    {
        {"fireball",6},
        {"greater fireball",10},
        {"heal",6},
        {"agility",2}
    };

    public static string SkillcostStr(string skillname)
    {
        return skillcosts[skillname] + "";
    }

    public static void Perform(Fighter f, Fighter e, string skillname)
    {
        switch (skillname)
        {
            case "fireball":
                Fireball(f, e);
                break;
            case "greater fireball":
                GreaterFireball(f, e);
                break;
            case "heal":
                Heal(f);
                break;
            case "agility":
                Agility(f);
                break;
        }
    }

    public static bool TargetIsSelf(string skillname)
    {
        switch (skillname)
        {
            case "fireball":
                return false;
            case "greater fireball":
                return false;
            case "heal":
                return true;
            case "agility":
                return true;
            default:
                return true;
        }
    }

    private static void Heal(Fighter f)
    {
        if (f.mp >= skillcosts["heal"])
        {
            f.mp -= skillcosts["heal"];
            f.hp += Convert.ToInt32(Math.Floor(f.maxHp / 4.0));
            if (f.hp > f.maxHp)
            {
                f.hp = f.maxHp;
            }
        }
        else
            Console.WriteLine(fail);
    }

    private static void Fireball(Fighter f, Fighter e)
    {
        if (f.mp >= skillcosts["fireball"])
        {
            f.mp -= skillcosts["fireball"];
            e.hp -= Convert.ToInt32(Math.Floor(Battle.CalcDmg(f.tempAtk, e.tempMdef) * 1.1));
        }
        else
            Console.WriteLine(fail);
    }

    private static void GreaterFireball(Fighter f, Fighter e)
    {
        if (f.mp >= skillcosts["greater fireball"])
        {
            f.mp -= skillcosts["greater fireball"];
            e.hp -= Convert.ToInt32(Math.Floor(Battle.CalcDmg(f.tempAtk, e.tempMdef) * 1.35));
        }
        else
            Console.WriteLine(fail);
    }

    private static void Agility(Fighter f)
    {
        if (f.mp >= skillcosts["agility"])
        {
            f.mp -= skillcosts["agility"];
            f.tempSpd *= 1.5;
        }
        else
            Console.WriteLine(fail);
    }
}

public static class Battle
{
    public static void HpMpShow(Fighter f, Fighter e)
    {
        Console.WriteLine(f.name + ": " + f.hp + "/" + f.maxHp + "HP || " + e.name + ": " + e.hp + "/" + e.maxHp + "HP");
        int spaces = f.name.Length;
        int spacesE = e.name.Length;
        string spacesStr = "  ";
        string spacesStrE = "  ";

        for (int o = 0; o < spaces; o++)
        {
            spacesStr = spacesStr + " ";
        }
        for (int o = 0; o < spacesE; o++)
        {
            spacesStrE = spacesStrE + " ";
        }

        Console.WriteLine(spacesStr + f.mp + "/" + f.maxMp + "MP || " + spacesStrE + e.mp + "/" + e.maxMp + "MP");
    }

    public static void HpMpShow(Fighter f)
    {
        Console.WriteLine(f.name + ": " + f.hp + "/" + f.maxHp + "HP");
        int spaces = f.name.Length;
        string spacesStr = "  ";

        for (int o = 0; o < spaces; o++)
        {
            spacesStr = spacesStr + " ";
        }

        Console.WriteLine(spacesStr + f.mp + "/" + f.maxMp + "MP");
        Console.WriteLine();
    }

    public static void BattleMsg(StringBuilder choices, int arrayOneCount, params Fighter[] fs)
    {
        Console.Clear();
        int i = 0;
        do
        {
            if (i == 0)
                Console.Write(fs[0].name + ": " + fs[0].hp + "/" + fs[0].maxHp + "HP");
            else
                Console.Write(" || " + fs[i].name + ": " + fs[i].hp + "/" + fs[i].maxHp + "HP");
            i++;
        } while (i < fs.Length);
        Console.WriteLine();
        i = 0;
        do
        {
            int spaces;
            string spacesStr = "  ";
            if (i == 0)
            {
                spaces = fs[0].name.Length;
                for (int o = 0; o < spaces; o++)
                {
                    spacesStr = spacesStr + " ";
                }
                Console.Write(spacesStr + fs[0].mp + "/" + fs[0].maxMp + "MP");
            }
            else
            {
                spaces = fs[i].name.Length;
                for (int o = 0; o < spaces; o++)
                {
                    spacesStr = spacesStr + " ";
                }
                Console.Write(" || " + spacesStr + fs[i].mp + "/" + fs[i].maxMp + "MP");
            }
            i++;
        } while (i < fs.Length);
        Console.WriteLine();
        if (choices.ToString().Length > 0)
        {
            Console.Write(choices.ToString());
            Console.WriteLine();
        }
    }

    public static int CalcDmg(double atk, double def, int doNotFillPls = 1)
    {
        doNotFillPls = Convert.ToInt32(Math.Floor((double)atk - (double)def / 2.0));
        if (doNotFillPls != 0)
            return doNotFillPls;
        else
            return 1;
    }

    public static void InitiateStats(params Fighter[] fighters)
    {
        foreach (Fighter f in fighters)
        {
            f.hp = f.maxHp;
            f.mp = f.maxMp;
            f.tempAtk = f.atk;
            f.tempDef = f.def;
            f.tempMdef = f.mdef;
            f.tempSpd = f.spd;
        }
    }

    public static string PlayerChoice(Fighter f, Fighter[] es, List<Item> inv)
    {
        if (f.hp <= 0)
            return "";

        bool chosen = false;
        bool checkInt;
        while (chosen != true)
        {
            int i;
            string chosenNum;
            int chosenNumParsed;
            Console.WriteLine("What should " + f.name + " do?");
            Console.WriteLine("Attack: 1");
            Console.WriteLine("Skills: 2");
            Console.WriteLine("Items:  3");
            Console.Write("Choose >> ");
            switch (Console.ReadLine())
            {
                case "1":
                    f.choice = 1;
                    do
                    {
                        i = 0;
                        foreach (Fighter e in es)
                        {
                            i++;
                            if (e.hp <= 0)
                                Console.WriteLine(e.name + ": " + i + "[DEAD]");
                            else
                                Console.WriteLine(e.name + ": " + i);
                        }
                        Console.Write("Choose the enemy to attack or go back with <n> >> ");
                        do
                        {
                            chosenNum = Console.ReadLine();
                            checkInt = true;
                            if (!int.TryParse(chosenNum, out chosenNumParsed))
                            {
                                if (chosenNum != "n")
                                {
                                    checkInt = false;
                                    Console.WriteLine("No number entered!");
                                    Console.Write("Please input a valid number or <n> to go back >>");
                                }
                            }
                        } while (checkInt == false);

                        if (chosenNum != "n")
                        {
                            if (chosenNumParsed < 1 || chosenNumParsed > es.Length)
                            {
                                Console.WriteLine("Enemy doesn't exist!");
                            }
                            else if (es[chosenNumParsed - 1].hp <= 0)
                            {
                                Console.WriteLine("Enemy is already dead!");
                            }
                            else if (chosenNum != "n")
                            {
                                chosen = true;
                                f.foeIndex = chosenNumParsed - 1;
                            }
                        }
                    } while (chosenNum != "n" && chosen != true);
                    break;
                case "2":
                    f.choice = 2;
                    bool attackExists = false;
                    string skillchoice;
                    do
                    {
                        i = 0;
                        foreach (string s in f.skillset)
                        {
                            i++;
                            Console.WriteLine(s + "(" + Skills.SkillcostStr(s) + "): " + i);
                        }

                        Console.Write("Choose the skill or go back with <n> >> ");
                        int skillchoiceParsed = 1;
                        do
                        {
                            skillchoice = Console.ReadLine();
                            checkInt = true;
                            if (!int.TryParse(skillchoice, out skillchoiceParsed))
                            {
                                if (skillchoice != "n")
                                {
                                    checkInt = false;
                                    Console.WriteLine("No number entered!");
                                    Console.Write("Please input a valid number or <n> to go back >>");
                                }
                            }
                        } while (checkInt == false);

                        if (skillchoice != "n")
                        {
                            for (int foo = 1; foo <= i; foo++)
                            {
                                if (foo == skillchoiceParsed)
                                {
                                    attackExists = true;
                                }
                            }
                            if (attackExists == true)
                            {
                                f.magiChoice = skillchoiceParsed - 1;
                                if (!Skills.TargetIsSelf(f.skillset[f.magiChoice]))
                                {
                                    do
                                    {
                                        i = 0;
                                        foreach (Fighter e in es)
                                        {
                                            i++;
                                            if (e.hp <= 0)
                                                Console.WriteLine(e.name + ": " + i + "[DEAD]");
                                            else
                                                Console.WriteLine(e.name + ": " + i);
                                        }
                                        Console.Write("Choose the enemy to attack or go back with <n> >> ");
                                        do
                                        {
                                            chosenNum = Console.ReadLine();
                                            checkInt = true;
                                            if (!int.TryParse(chosenNum, out chosenNumParsed))
                                            {
                                                if (chosenNum != "n")
                                                {
                                                    checkInt = false;
                                                    Console.WriteLine("No number entered!");
                                                    Console.Write("Please input a valid number or <n> to go back >>");
                                                }
                                            }
                                        } while (checkInt == false);

                                        if (chosenNum != "n")
                                        {
                                            if (chosenNumParsed < 1 || chosenNumParsed > es.Length)
                                            {
                                                Console.WriteLine("Enemy doesn't exist!");
                                            }
                                            else if (es[chosenNumParsed - 1].hp <= 0)
                                            {
                                                Console.WriteLine("Enemy is already dead!");
                                            }
                                            else if (chosenNum != "n")
                                            {
                                                chosen = true;
                                                f.foeIndex = chosenNumParsed - 1;
                                            }
                                        }
                                    } while (chosenNum != "n" && chosen != true);
                                }
                                else
                                {
                                    chosen = true;
                                    f.foeIndex = -1;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Skill does not exist!");
                            }
                        }
                    } while (attackExists != true && skillchoice != "n");
                    break;
                case "3":
                    f.choice = 3;
                    if (inv.Count == 0)
                    {
                        Console.WriteLine("No items in your inventory!");
                        break;
                    }
                    do
                    {
                        i = 0;
                        foreach (Item it in inv)
                        {
                            i++;
                            if (it.reserved)
                                Console.WriteLine(it.name + " [RESERVED]: " + i);
                            else
                            {
                                Console.WriteLine(it.name + ": " + i);
                            }
                        }
                        Console.Write("Choose an item to consume or go back with <n> >> ");
                        do
                        {
                            chosenNum = Console.ReadLine();
                            checkInt = true;
                            if (!int.TryParse(chosenNum, out chosenNumParsed))
                            {
                                if (chosenNum != "n")
                                {
                                    checkInt = false;
                                    Console.WriteLine("No number entered!");
                                    Console.Write("Please input a valid number or <n> to go back >>");
                                }
                            }
                        } while (checkInt == false);

                        if (chosenNum != "n")
                        {
                            if (chosenNumParsed < 1 || chosenNumParsed > inv.Count)
                            {
                                Console.WriteLine("Item doesn't exist!");
                            }
                            else if (inv[chosenNumParsed - 1].reserved)
                            {
                                Console.WriteLine("Item is already reserved for use!");
                            }
                            else if (chosenNum != "n")
                            {
                                inv[chosenNumParsed - 1].reserved = true;
                                chosen = true;
                                f.itemChoice = chosenNumParsed - 1;
                            }
                        }
                    } while (chosenNum != "n" && chosen != true);
                    break;
                default:
                    Console.WriteLine("Invalid number entered! Please input a valid number.");
                    break;
            }
        }

        if (f.choice == 1)
            return f.name + " attacks " + es[f.foeIndex].name;
        else if (f.choice == 2 && Skills.TargetIsSelf(f.skillset[f.magiChoice]))
            return f.name + " casts " + f.skillset[f.magiChoice];
        else if (f.choice == 2)
            return f.name + " casts " + f.skillset[f.magiChoice] + " on " + es[f.foeIndex].name;
        else
            return f.name + " consumes " + inv[f.itemChoice].name;
    }

    public static void EnemyChoice(Fighter f, Fighter[] es)
    {
        if (f.hp <= 0)
            return;

        Random rnd = new Random();
        int lol = 0;
        while (lol < 1 || lol > 3)
        {
            lol = rnd.Next(0, 3);
        }
        bool enemyDead = false;
        switch (lol)
        {
            case 1:
                f.choice = 1;
                do
                {
                    enemyDead = false;
                    do
                    {
                        lol = rnd.Next(-1, es.Length);
                    } while (lol < 0 || lol >= es.Length);
                    if (es[lol].hp <= 0)
                        enemyDead = true;
                } while (enemyDead);
                f.foeIndex = lol;
                break;
            case 2:
                f.choice = 2;
                do
                {
                    lol = rnd.Next(-1, f.skillset.Length);
                } while (lol < 0 || lol >= f.skillset.Length);
                f.magiChoice = lol;
                if (Skills.TargetIsSelf(f.skillset[f.magiChoice]))
                {
                    f.foeIndex = -1;
                    break;
                }
                do
                {
                    enemyDead = false;
                    do
                    {
                        lol = rnd.Next(-1, es.Length);
                    } while (lol < 0 || lol >= es.Length);
                    if (es[lol].hp <= 0)
                        enemyDead = true;
                } while (enemyDead);
                f.foeIndex = lol;
                break;
        }
    }

    public static void Turn(Fighter f, Fighter f2, Fighter[] ps, Fighter[] es, List<Item> inv, bool aSelfCast = false)
    {
        if (f.hp <= 0)
            return;

        switch (f.choice)
        {
            case 1:
                Console.WriteLine(f.name + " attacks " + f2.name + "!");
                f2.hp -= CalcDmg(f.tempAtk, f2.tempDef);
                HpMpShow(f, f2);
                Wincon(ps, es);
                break;
            case 2:
                Skills.Perform(f, f2, f.skillset[f.magiChoice]);
                if (aSelfCast)
                {
                    Console.WriteLine(f.name + " casts " + f.skillset[f.magiChoice] + ".");
                    HpMpShow(f);
                }
                else
                {
                    Console.WriteLine(f.name + " casts " + f.skillset[f.magiChoice] + " on " + f2.name + ".");
                    HpMpShow(f, f2);
                }
                Wincon(ps, es);
                break;
            case 3:
                Console.WriteLine(f.name + " consumes one " + inv[f.itemChoice].name);
                inv[f.itemChoice].Consume(f);
                break;
        }

        f.choice = 0;
        f.magiChoice = 0;
        f.itemChoice = 0;
    }

    public static void Wincon(Fighter[] f1s, Fighter[] f2s)
    {
        int alivecount1 = f1s.Length;
        int alivecount2 = f2s.Length;

        foreach (Fighter f in f1s)
        {
            if (f.hp <= 0)
                alivecount1--;
        }

        foreach (Fighter f in f2s)
        {
            if (f.hp <= 0)
                alivecount2--;
        }

        if (alivecount2 > alivecount1 && alivecount1 <= 0)
        {
            Console.WriteLine("The players lost. The monsters won the battle!");
            Console.ReadLine();
            Environment.Exit(0);
        }
        if (alivecount1 > alivecount2 && alivecount2 <= 0)
        {
            Console.WriteLine("The monsters lost. The players won the battle!");
            Console.ReadLine();
            Environment.Exit(0);
        }
    }
}

public class Item
{
    public readonly string name;
    public readonly string type;
    public readonly double valnum;
    public bool reserved = false;

    public Item(string cName, string cType, double leVal)
    {
        name = cName;
        type = cType;
        valnum = leVal;
    }

    public void Consume(Fighter f)
    {
        switch (type)
        {
            case "hp":
                f.hp += Convert.ToInt32(valnum);
                if (f.hp > f.maxHp)
                    f.hp = f.maxHp;
                Console.WriteLine(f.name + "'s HP were restored.");
                break;
            case "mp":
                f.mp += Convert.ToInt32(valnum);
                if (f.mp > f.maxMp)
                    f.mp = f.maxMp;
                Console.WriteLine(f.name + "'s MP were restored.");
                break;
            case "atk":
                f.tempAtk *= valnum;
                Console.WriteLine(f.name + "'s attacks got stronger.");
                break;
            case "def":
                f.tempDef *= valnum;
                Console.WriteLine(f.name + "'s defense got stronger.");
                break;
            case "mdef":
                f.tempMdef *= valnum;
                Console.WriteLine(f.name + "'s magical defense got stronger.");
                break;
            case "spd":
                f.tempSpd *= valnum;
                Console.WriteLine(f.name + "'s speed got stronger.");
                break;
        }
    }
}