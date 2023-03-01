using System;
using System.Diagnostics;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography.X509Certificates;
using NAudio.Midi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Drawing.Imaging;
using System.Drawing;

namespace классы1
{
    class Audio_info
    {
        private string title;
        private int duration;

        //свойства
        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                title = value;
                if (value.Length == 0)
                {
                    title = "Track_Placeholder";
                }
            }
        }

        public int Duration
        {
            get
            {
                return duration;
            }

            set
            {
                duration = value;
                if (value < 0)
                {
                    duration = 0;
                }
            }
        }

        //конструкторы
        public Audio_info(string a, int b)
        {
            title = a;
            duration = b;
        }

        public Audio_info(string a)
        {
            title = a;
            duration = 0;
        }

        public Audio_info(int b)
        {
            title = "Track_Placeholder";
            duration = b;
        }

        public Audio_info()
        {
            title = "Track_Placeholder";
            duration = 0;
        }

        //метод
        public void info_track()
        {
            int sec = duration;
            int min = sec / 60;
            if (sec > 60)
            {
                Console.WriteLine($"Название: {title}, Длина: {string.Format("{0:00}", min)}:{string.Format("{0:00}", sec - min * 60)}");
            }
            else if (sec > 0)
            {
                Console.WriteLine($"Название: {title}, Длина: {string.Format("{0:00}", min)}:{string.Format("{0:00}", duration)}");
            }
            else
            {
                Console.WriteLine($"Название: {title}, Длина: {string.Format("{0:00}", min)}:{string.Format("{0:00}", 0)}");
            }
        }

        public int cut_track(string inp, string outp, int a, int b, string ch)
        {
            if (duration >= 0 && a <= duration && b <= duration)
            {
                var new_duration = 0;
                switch (ch)
                {
                    case "1":
                        TrimMp3(inp, outp, TimeSpan.FromSeconds(a), TimeSpan.FromSeconds(b + 0.1), "1");
                        new_duration = duration - b - a;
                        duration = new_duration;
                        add_track(outp);
                        break;

                    case "2":
                        //cutTT(inp, outp, TimeSpan.FromSeconds(a), TimeSpan.FromSeconds(b));
                        TrimMp3(inp, outp, TimeSpan.FromSeconds(a), TimeSpan.FromSeconds(b + 0.1), "2");
                        new_duration = b - a;
                        duration = new_duration;
                        add_track(outp);
                        break;

                        /* var trimmed = new AudioFileReader(inp)
                       .Skip(TimeSpan.FromSeconds(a))
                       .Take(TimeSpan.FromSeconds(b - a));
                         WaveFileWriter.CreateWaveFile16(outp, trimmed);
                        */
                }

                Console.WriteLine("Хотите прослушать новую дорожку?");
                Console.WriteLine("1. Да");
                Console.WriteLine("2. Нет");
                var ch2 = Console.ReadLine();
                if (ch2 == "1")
                {
                    playS(outp, ch2);
                }
                //duration = duration - new_duration;
                return duration;
            }
            else
            {
                return 0;
            }
        }

        void TrimMp3(string inputPath, string outputPath, TimeSpan? begin, TimeSpan? end, string ch)
        {
            if (begin.HasValue && end.HasValue && begin > end)
                throw new ArgumentOutOfRangeException("end", "end should be greater than begin");

            switch (ch)
            {
                case "1":

                    using (AudioFileReader readerWV = new AudioFileReader(inputPath))
                    {
                        TimeSpan startPosition = (TimeSpan)begin;
                        TimeSpan endPosition = (TimeSpan)end;
                        IWaveProvider wave = CutAudio(readerWV, startPosition, endPosition);

                        WaveFileWriter.CreateWaveFile(outputPath, wave);
                    }

                    break;

                case "2":

                    using (var reader = new Mp3FileReader(inputPath))
                    using (var writer = File.Create(outputPath))
                    {
                        Mp3Frame frame;
                        while ((frame = reader.ReadNextFrame()) != null)
                        {
                            if (reader.CurrentTime >= begin || !begin.HasValue)
                            {
                                if (reader.CurrentTime <= end || !end.HasValue)
                                    writer.Write(frame.RawData, 0, frame.RawData.Length);
                                else break;
                            }
                        }
                    }
                    break;
            }


            /*if (begin.HasValue && end.HasValue && begin > end)
                throw new ArgumentOutOfRangeException("end", "end should be greater than begin");

            using (var reader = new Mp3FileReader(inputPath))
            using (var writer = File.Create(outputPath))
            {
                Mp3Frame frame;
                while ((frame = reader.ReadNextFrame()) != null)
                    if (reader.CurrentTime >= begin || !begin.HasValue)
                    {
                        if (reader.CurrentTime <= end || !end.HasValue)
                            writer.Write(frame.RawData, 0, frame.RawData.Length);
                        else break;
                    }
            }
            */
        }

        public static IWaveProvider CutAudio(WaveStream wave,
                                   TimeSpan startPosition,
                                   TimeSpan endPosition)
        {

            ISampleProvider sourceProvider = wave.ToSampleProvider();
            long currentPosition = wave.Position; // Save stream position

            // Take audio from the beginning of file until {startPosition}
            OffsetSampleProvider offset1 = new OffsetSampleProvider(sourceProvider)
            {
                Take = startPosition
            };

            // Take audio after {endPosition} until the end of file
            OffsetSampleProvider offset2 = new OffsetSampleProvider(sourceProvider)
            {
                SkipOver = (endPosition - startPosition),
            };

            wave.Position = currentPosition; // Restore stream position
            return (offset1.FollowedBy(offset2)).ToWaveProvider();

        }


        public void playS(string a, string c)
        {
            var reader = new AudioFileReader(a);
            var waveOut = new WaveOutEvent();
            waveOut.Init(reader);
            switch (c)
            {
                case "1": waveOut.Play(); break;
                case "2": waveOut.Pause(); break;
                case "0": waveOut.Stop(); break;
            }
        }

        public void add_track(string outp)
        {
            var readerF = new AudioFileReader(outp);
            var fileName = Path.GetFileName(outp);
            Audio_info trackNew = new Audio_info();
            trackNew.Title = fileName;
            trackNew.Duration = readerF.TotalTime.Minutes * 60 + readerF.TotalTime.Seconds;

            trackNew.info_track();

            //Audio_info trackNew = new Audio_info(fileName, readerF.TotalTime.Minutes * 60 + readerF.TotalTime.Seconds);
            //trackNew.info_track();
        }
    }

    class ChangeTrackPath: Audio_info
    {
        private string newPath;

        public string NewPath {
            get
            {
                return newPath;
            }

            set
            {
                newPath = value;
                if (value.Length < 0) {
                    newPath = "Track_Placeholder";
                }
            }
        }

        public ChangeTrackPath(string inpP, int d, string newP) : base(inpP, d)
        {
            NewPath = newP;
        }

        public void show_new_path()
        {
            Console.WriteLine($"Новый путь (ChangeTrackPath): {NewPath}");
        }
    }

    class CreateAudioWave : Audio_info
    {
        private string newPath;

        public string NewPath
        {
            get
            {
                return newPath;
            }

            set
            {
                newPath = value;
                if (value.Length < 0)
                {
                    newPath = "Track_Placeholder";
                }
            }
        }

        public CreateAudioWave(string inpP, int d, string newP) : base(inpP, d)
        {
            NewPath = newP;
        }

        public void show_new_path()
        {
            Console.WriteLine($"Новый путь (ChangeTrackPath): {NewPath}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string sourceFile = "C:\\Users\\klausreinherz\\Downloads\\test.mp3";
            var reader = new AudioFileReader(sourceFile);
            var fileName = Path.GetFileName(sourceFile);
            //Audio_info track1 = new Audio_info(fileName, reader.TotalTime.Minutes * 60 + reader.TotalTime.Seconds);

            Audio_info track1 = new Audio_info();
            track1.Title = fileName;
            track1.Duration = reader.TotalTime.Minutes * 60 + reader.TotalTime.Seconds;
            
            track1.info_track();
            Console.WriteLine($"Старый путь(вызов writeline): {sourceFile}");

            ChangeTrackPath track1NP = new ChangeTrackPath(track1.Title, track1.Duration, "C:\\Users\\klausreinherz\\Downloads\\NEWPATH.mp3");
            track1NP.show_new_path();

            var waveOut = new WaveOutEvent();
            waveOut.Init(reader);

            Console.WriteLine("1. Прослушать трек");
            Console.WriteLine("2. Приостановить трек");
            Console.WriteLine("1.1. Редактировать трек");
            Console.WriteLine("0. Закончить работу");
            
            var a = "";
            a = Console.ReadLine();
            
            while(a != "0")
            {
                switch (a)
                {
                    case "1": 
                        waveOut.Play();
                        break;
                    case "1.1":
                        Console.WriteLine("1. Вырезать(удалить) промежуток из исходной дорожки");
                        Console.WriteLine("2. Вырезать промежуток как новую дорожку");
                        var ch = Console.ReadLine();
                        Console.WriteLine("Задайте промежуток в секундах:");
                        Console.WriteLine("Начало:");
                        var s = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("Конец: ");
                        var e = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("Введите название новой дорожки:");
                        var n = Console.ReadLine();
                        string outputFile = $"C:\\Users\\klausreinherz\\Downloads\\{n}.mp3";
                        Console.WriteLine($"Новая длина: {track1.cut_track(sourceFile, outputFile, s, e, ch)} сек.");
                        break;
                    case "2":
                        waveOut.Pause();
                        break;
                    case "0":
                        waveOut.Stop();
                        break;
                }
                a = Console.ReadLine();
            }

            Console.WriteLine("Конец работы.");
            Console.ReadKey();
        }
    }
} 

/*void cutTT(string inputPath, string outputPath, TimeSpan begin, TimeSpan end)
       {
           using (var input = new Mp3FileReader(inputPath))
           {
               // Create an output file with the same format as the input file
               //(var output = new Mp3FileWriter("output.mp3", input.WaveFormat)
               using (var output = File.Create(outputPath))
               {
                   // Set the input file's position to the start time
                   input.CurrentTime = begin;

                   // Read and write audio data from the input file to the output file
                   while (input.CurrentTime < end)
                   {
                       var buffer = new byte[input.WaveFormat.AverageBytesPerSecond
                           * (end - input.CurrentTime).Seconds];
                       var bytesRead = input.Read(buffer, 0, buffer.Length);

                       if (bytesRead == 0)
                       {
                           break;
                       }

                       output.Write(buffer, 0, bytesRead);
                   }
               }
           }
       } */


//2 глава должна быть готова к 12(20) апреля (печатать), список литературы тоже дожен быть
//курсовая работа должна быть готова к 18 мая
// 10 мая можно сдать раньше
// титульный лист
// введение - 1-2 страницы
// обязательно лист содержания
// главы - 4-6 страниц
// заключение - 1 страница
// список литературы(по госту), количество источников - не менее 3, в главах ссылка на каждый источник
