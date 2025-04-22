using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;


internal class run
{
    private static bool CheckCapacity(int maxCapacity, List<Guest> guests)
    {
        var events = new List<(string, int)>();
        foreach (var guest in guests)
        {
            events.Add((guest.CheckIn, 1));
            events.Add((guest.CheckOut, -1));
        }
        
        events.Sort();
        
        var count = 0;
        foreach (var guest in events)
        {
            count += guest.Item2;
            if (count > maxCapacity)
                return false;
        }
        
        return true;
    }


    private class Guest
    {
        public string Name { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
    }


    private static void Main()
    {
        var maxCapacity = int.Parse(Console.ReadLine());
        var n = int.Parse(Console.ReadLine());


        var guests = new List<Guest>();


        for (var i = 0; i < n; i++)
        {
            var line = Console.ReadLine();
            var guest = ParseGuest(line);
            guests.Add(guest);
        }


        var result = CheckCapacity(maxCapacity, guests);


        Console.WriteLine(result ? "True" : "False");
    }


    // Простой парсер JSON-строки для объекта Guest 
    private static Guest ParseGuest(string json)
    {
        var guest = new Guest();


        // Извлекаем имя
        var nameMatch = Regex.Match(json, "\"name\"\\s*:\\s*\"([^\"]+)\"");
        if (nameMatch.Success)
            guest.Name = nameMatch.Groups[1].Value;


        // Извлекаем дату заезда
        var checkInMatch = Regex.Match(json, "\"check-in\"\\s*:\\s*\"([^\"]+)\"");
        if (checkInMatch.Success)
            guest.CheckIn = checkInMatch.Groups[1].Value;


        // Извлекаем дату выезда
        var checkOutMatch = Regex.Match(json, "\"check-out\"\\s*:\\s*\"([^\"]+)\"");
        if (checkOutMatch.Success)
            guest.CheckOut = checkOutMatch.Groups[1].Value;


        return guest;
    }
}