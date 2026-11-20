using System;

public class Song
{
    public string Title;
    public string Author;
    public Song PreviousSong;

    public Song(string title, string author)
    {
        Title = title;
        Author = author;
        PreviousSong = null;
    }

    public Song(string title, string author, Song previousSong)
    {
        Title = title;
        Author = author;
        PreviousSong = previousSong;
    }


    public override string ToString()
    {
        return $"Песня: {Title}, Исполнитель: {Author}" + 
               (PreviousSong != null ? $", Предыдущая: {PreviousSong.Title}" : "");
    }
}


class Program
{
    static void Main()
    {
        Song song1 = new Song("Yesterday", "The Beatles");
        Song mySong = new Song("Линия жизни", "Сплин", song1);

        Console.WriteLine("========================");
        Console.WriteLine(mySong);
    }
}