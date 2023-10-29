using System.Data.SqlTypes;

string[] countries =
{
    "Вландия",
    "Стургия",
    "Империя",
    "Асераи",
    "Хузаиты",
    "Баттания"
};

string[] sex = { "М", "Ж" };

const int maxOption = 6;
string[] options =
{
    "Семья",
    "Раннее детство",
    "Отрочество",
    "Ранняя юность",
    "Юность"
};
const int minLookRoll = 3;
const int maxLookRoll = 10;

int[] age = { 20, 30, 40, 50 };

const int pictCount = 227;
const int maxRowCount = 5;
const int colorCount = 8;

Random random = new();

while(true)
{
    Console.WriteLine($"Country: {countries[random.Next(countries.Length)]}");
    Console.WriteLine($"Sex: {sex[random.Next(sex.Length)]}");
    Console.WriteLine($"Look rolls: {random.Next(minLookRoll, maxLookRoll + 1)}");
    foreach (var opt in options)
        Console.WriteLine($"Option {opt}: {random.Next(maxOption) + 1}");
    Console.WriteLine($"Age: {age[random.Next(age.Length)]}");
    int pict = random.Next(pictCount);
    int row = pict / maxRowCount + 1;
    int column = pict % maxRowCount + 1;
    Console.WriteLine($"Picture row= {row} column= {column}");
    Console.WriteLine($"Inverse? {random.Next(2) == 0}");
    Console.WriteLine($"Top color: {random.Next(colorCount) + 1}");
    Console.WriteLine($"Bottom color: {random.Next(colorCount) + 1}");
    Console.ReadKey();
    Console.Clear();
}

