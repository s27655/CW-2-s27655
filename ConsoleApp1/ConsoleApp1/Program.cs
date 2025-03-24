
public interface IHazardNotifier
{
    void NotifyHazard(string serialNumber, string message);
}
public abstract class Container
{
    private static Dictionary<string, int> serialNumberCounter = new Dictionary<string, int>()
    {
        { "C", 1 },
        { "L", 1 },
        { "G", 1 }
    };

    public string SerialNumber { get; private set; }
    public double CargoWeight { get; protected set; }
    public double Height { get; private set; }
    public double OwnWeight { get; private set; }
    public double Depth { get; private set; }
    public double MaxCapacity { get; private set; }
    public Container(string containerType, double height, double ownWeight, double depth, double maxCapacity)
    {
        SerialNumber = $"KON-{containerType}-{serialNumberCounter[containerType]++}";
        Height = height;
        OwnWeight = ownWeight;
        Depth = depth;
        MaxCapacity = maxCapacity;
    }
    

    public virtual string getInfo()
    {
        return $"Kontener {SerialNumber}\n" +
               $"Wymiary: {Height}cm x {Depth}cm\n" +
               $"Waga własna: {OwnWeight}kg\n" +
               $"Ładunek: {CargoWeight}kg / {MaxCapacity}kg";
    }
    public virtual void LoadCargo(double cargoWeight)
    {
        if (cargoWeight <= MaxCapacity)
        {
            CargoWeight = cargoWeight;
        }
        else
        {
            throw new OverfillException($"Próba załadowania {cargoWeight} kg przekracza maksymalną pojemność {MaxCapacity} kg");
        }
    }

    public virtual void emptyCargo()
    {
        CargoWeight = 0;
    }
    
}

public class OverfillException : Exception
{
    public OverfillException(string s) : base(s)
    {
    }
}
public class RefrigeratorContainer: Container
{
    public string Type { get; private set; }
    public double Temperature { get; private set; }
    
    public RefrigeratorContainer(double height, double ownWeight, double depth
        , double maxCapacity,string type, double temperature) 
        : base("C", height, ownWeight, depth, maxCapacity)
    {
        Type = type;
        Temperature = temperature;
    }

    public override void LoadCargo(double cargoWeight)
    {
        if (cargoWeight <= MaxCapacity)
        {
            CargoWeight = cargoWeight;
        }
        else
        {
            throw new OverfillException($"Próba załadowania {cargoWeight} kg przekracza maksymalną pojemność {MaxCapacity} kg");
        }
    }

    public override string getInfo()
    {
        return base.getInfo() + $"\nTyp: {Type}\nTemperatura: {Temperature} stopni Celsius";
    }
}

public class LiquidContainer : Container, IHazardNotifier
{
    public bool IsHazardous { get; private set; }

    public LiquidContainer(double height, double ownWeight, double depth
        , double maxCapacity, bool isHazardous)
        : base("L", height, ownWeight, depth, maxCapacity)
    {
        IsHazardous = isHazardous;
    }

    public void NotifyHazard(string serialNumber, string message)
    {
        Console.WriteLine($"Niebezpieczny kontener {serialNumber}: {message}");
    }

    public override string getInfo()
    {
        return base.getInfo() + $"\nNiebezpieczny: {IsHazardous}";
    }

    public override void LoadCargo(double cargoWeight)
    {
        double maxAllowedWeight;

        if (IsHazardous)
        {
            maxAllowedWeight = MaxCapacity * 0.5;
        }
        else
        {
            maxAllowedWeight = MaxCapacity * 0.9;
        }

        if (cargoWeight > maxAllowedWeight)
        {
            NotifyHazard(SerialNumber,
                $"Próba przekroczenia bezpiecznego poziomu załadunku: {cargoWeight}kg > {maxAllowedWeight}kg");
            throw new OverfillException(
                $"Próba załadowania {cargoWeight} kg przekracza bezpieczny poziom {maxAllowedWeight} kg");
        }

        if (cargoWeight > MaxCapacity)
        {
            throw new OverfillException(
                $"Próba załadowania {cargoWeight} kg przekracza maksymalną pojemność {MaxCapacity} kg");
        }

        CargoWeight = cargoWeight;
    }
}
public class GasContainer : Container, IHazardNotifier
{
    public double Pressure { get; private set; }
    
    public GasContainer(double height, double ownWeight, double depth
        , double maxCapacity, double pressure)
        : base("G", height, ownWeight, depth, maxCapacity)
    {
        Pressure = pressure;
    }
   

    public override void LoadCargo(double cargoWeight)
    {
        if (cargoWeight > MaxCapacity)
        {
            NotifyHazard(SerialNumber, $"Próba przekroczenia maksymalnej pojemności: {cargoWeight}kg > {MaxCapacity}kg");
        }
    }
    

    public override void emptyCargo()
    {
        CargoWeight = MaxCapacity * 0.05;
    }
    
    public void NotifyHazard(string serialNumber, string message)
    {
        Console.WriteLine($"Niebezpieczny kontener {serialNumber}: {message}");
    }

    public override string getInfo()
    {
        return base.getInfo()+$"\nCiśnienie: {Pressure} Pa";
    }
}

public class Ship
{
    public string Name { get; private set; }
    public double MaxSpeed { get; private set; }
    public int MaxContainerCount { get; private set; } 
    public double MaxContainerWeight { get; private set; }
    public List<Container> Containers { get; private set; }
    
    public Ship(string name, double maxSpeed, int maxContainerCount, double maxContainerWeight)
    {
        Name = name;
        MaxSpeed = maxSpeed;
        MaxContainerCount = maxContainerCount;
        MaxContainerWeight = maxContainerWeight;
        Containers = new List<Container>();
    }
    public bool LoadContainer(Container container)
    {
        if (Containers.Count >= MaxContainerCount)
        {
            Console.WriteLine($"Nie można załadować kontenera {container.SerialNumber}. Przekroczona maksymalna ilość kontenerów");
            return false;
        }
    
        double currentWeight = GetTotalWeight();
        double newWeight = container.OwnWeight + container.CargoWeight;
    
        if (currentWeight + newWeight > MaxContainerWeight)
        {
            Console.WriteLine($"Nie można załadować kontenera {container.SerialNumber}. Przekroczono maksymalną wagę kontenerów {MaxContainerWeight} kg.");
            return false;
        }
    
        currentWeight += newWeight;
        Containers.Add(container);
        Console.WriteLine($"Kontener {container.SerialNumber} został załadowany na statek");
        return true;
    }

    public void LoadContainers(List<Container> containers)
    {
        foreach (var container in containers)
        {
            LoadContainer(container);
        }
    }

    public bool UnloadContainer(string serialNumber)
    {
        Container container = Containers.Find(c => c.SerialNumber == serialNumber);

        if (container != null)
        {
            Containers.Remove(container);
            Console.WriteLine($"Kontener {serialNumber} został usunięty ze statku");
            return true;
        }
        Console.WriteLine($"Kontener {serialNumber} nie istnieje na statku");
        return false;
    }

    public bool ReplaceContainer(string serialNumber, Container newContainer)
    {
        int index = Containers.FindIndex(c => c.SerialNumber == serialNumber);

        if (index >= 0)
        {
            double currentWeight = GetTotalWeight();
            double oldWeight = Containers[index].OwnWeight + Containers[index].CargoWeight;
            double newWeight = newContainer.OwnWeight + newContainer.CargoWeight;

            if (currentWeight - oldWeight +newWeight > MaxContainerWeight)
            {
                Console.WriteLine($"Nie można zamienić kontenera {serialNumber}. Przekroczono max wage");
                return false;
            }

            Containers[index] = newContainer;
            Console.WriteLine($"Kontemer {serialNumber} został zastąpiony nowym kontenerem {newContainer.SerialNumber}");
            return true;
        }
        Console.WriteLine($"Kontener {serialNumber} nie istnieje na statku");
        return false;
    }

    private double GetTotalWeight()
    {
        return Containers.Sum(container => container.OwnWeight + container.CargoWeight);
        
    }
}

public class ContainerManagementSystem
{
    private List<Container> containers;
    private List<Ship> ships;

    public ContainerManagementSystem()
    {
        containers = new List<Container>();
        ships = new List<Ship>();
    }

    public void AddContainer(Container container)
    {
        containers.Add(container);
        Console.WriteLine($"Dodano kontener {container.SerialNumber}");
    }

    public void ListAvailableContainers()
    {
        if (containers.Count == 0)
        {
            Console.WriteLine("Brak dostępnych kontenerów");
            return;
        }

        Console.WriteLine("Lista dostęnych kontenerów:");
        foreach (var container in containers)
        {
            Console.WriteLine(container.SerialNumber);
        }
    }

    public void DisplayContainerInfo(string serialNumber)
    {
        Container container = containers.Find(c => c.SerialNumber == serialNumber);
        
        if (container != null)
        {
            Console.WriteLine("Informacje o kontenerze (dostępny):");
            Console.WriteLine(container.getInfo());
            return;
        }
        foreach (var ship in ships)
        {
            container = ship.Containers.Find(c => c.SerialNumber == serialNumber);
            
            if (container != null)
            {
                Console.WriteLine($"Informacje o kontenerze (na statku {ship.Name}):");
                Console.WriteLine(container.getInfo());
                return;
            }
        }
        
        Console.WriteLine($"Nie znaleziono kontenera o numerze: {serialNumber}");
    }

    public void AddShip(Ship ship1)
    {
        ships.Add(ship1);
        Console.WriteLine($"Dodano statek {ship1.Name}");
    }

    public void ListShips()
    {
        if (ships.Count == 0)
        {
            Console.WriteLine("Brak statków");
            return;
        }

        Console.WriteLine("Lista kontenerowców:");
        foreach (var ship in ships)
        {
            Console.WriteLine(ship.Name);
        }
        
    }

    public void LoadContainerToShip(string serialNumber, string name)
    {
        Container container = containers.Find(c => c.SerialNumber == serialNumber);
        if (container == null)
        {
            Console.WriteLine($"Kontener {serialNumber} nie istnieje w magazynie.");
            return;
        }
    
        Ship ship = ships.Find(s => s.Name == name);
        if (ship == null)
        {
            Console.WriteLine($"Statek {name} nie istnieje.");
            return;
        }
    
        if (ship.LoadContainer(container))
        {
            containers.Remove(container);
        }
    }

    public void DisplayShipInfo(string name)
    {
        Ship ship = ships.Find(s => s.Name == name);
        if (ship == null)
        {
            Console.WriteLine($"Statek {name} nie istnieje.");
            return;
        }
    
        Console.WriteLine($"Informacje o statku: {ship.Name}");
        Console.WriteLine($"Maksymalna prędkość: {ship.MaxSpeed} km/h");
        Console.WriteLine($"Maksymalna liczba kontenerów: {ship.MaxContainerCount}");
        Console.WriteLine($"Maksymalna waga kontenerów: {ship.MaxContainerWeight} kg");
        Console.WriteLine("Załadowane kontenery:");
    
        if (ship.Containers.Count == 0)
        {
            Console.WriteLine("Brak załadowanych kontenerów.");
        }
        else
        {
            foreach (var container in ship.Containers)
            {
                Console.WriteLine($"- {container.SerialNumber}");
            }
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        ContainerManagementSystem system = new ContainerManagementSystem();
        bool running = true;
    
        while (running)
        {
            Console.WriteLine("\n=========== Menu ===========");
            Console.WriteLine("1: Add sample containers");
            Console.WriteLine("2: Add a ship");
            Console.WriteLine("3: Display all available containers");
            Console.WriteLine("4: Display all ships");
            Console.WriteLine("5: Load a container to a ship");
            Console.WriteLine("6: Unload a container from a ship");
            Console.WriteLine("7: Display container information");
            Console.WriteLine("8: Display ship information");
            Console.WriteLine("9: Exit");
            Console.WriteLine("============================");
            Console.Write("Choose an option: ");
            
            string choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    Console.WriteLine("Adding sample containers...");
                    system.AddContainer(new RefrigeratorContainer(250, 1000, 300, 5000, "Banany", 13.5));
                    system.AddContainer(new LiquidContainer(250, 800, 300, 3000, false));
                    system.AddContainer(new LiquidContainer(250, 1200, 300, 4000, true));
                    system.AddContainer(new GasContainer(250, 900, 300, 2000, 15.2));
                    Console.WriteLine("Sample containers added.");
                    break;
                case "2":
                    Console.Write("Enter ship name: ");
                    string shipName = Console.ReadLine();
                    Console.Write("Enter max speed (km/h): ");
                    double maxSpeed = double.Parse(Console.ReadLine());
                    Console.Write("Enter max container count: ");
                    int maxContainerCount = int.Parse(Console.ReadLine());
                    Console.Write("Enter max container weight (kg): ");
                    double maxContainerWeight = double.Parse(Console.ReadLine());
                    system.AddShip(new Ship(shipName, maxSpeed, maxContainerCount, maxContainerWeight));
                    Console.WriteLine($"Ship '{shipName}' added.");
                    break;
                case "3":
                    system.ListAvailableContainers();
                    break;
                case "4":
                    system.ListShips();
                    break;
                case "5":
                    Console.Write("Enter container serial number: ");
                    string containerSerial = Console.ReadLine();
                    Console.Write("Enter ship name: ");
                    string shipNameForLoad = Console.ReadLine();
                    try
                    {
                        system.LoadContainerToShip(containerSerial, shipNameForLoad);
                        Console.WriteLine($"Container {containerSerial} loaded to ship {shipNameForLoad}.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                    break;
                case "6":
                    Console.Write("Enter container serial number to unload: ");
                    string unloadSerial = Console.ReadLine();
                    Console.WriteLine("Functionality to unload container not implemented.");
                    break;
                case "7":
                    Console.Write("Enter container serial number: ");
                    string containerInfoSerial = Console.ReadLine();
                    system.DisplayContainerInfo(containerInfoSerial);
                    break;
                case "8":
                    Console.Write("Enter ship name: ");
                    string shipInfoName = Console.ReadLine();
                    system.DisplayShipInfo(shipInfoName);
                    break;
                case "9":
                    Console.WriteLine("Exiting...");
                    running = false;
                    break;
                default:
                    Console.WriteLine("Invalid option, please try again.");
                    break;
            }
        }
    }
}