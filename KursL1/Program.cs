using System;
using System.Diagnostics;
using System.IO;
using System.Reflection.PortableExecutable;
using NAudio.Midi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace классы1
{
    class Audio_info
    {
        string title;
        int duration;

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
                Console.WriteLine($"Title: {title}, Duration: {string.Format("{0:00}", min)}:{string.Format("{0:00}", sec - min * 60)}");
            }
            else if (sec > 0)
            {
                Console.WriteLine($"Title: {title}, Duration: {string.Format("{0:00}", min)}:{string.Format("{0:00}", duration)}");
            }
            else
            {
                Console.WriteLine($"Title: {title}, Duration: {string.Format("{0:00}", min)}:{string.Format("{0:00}", 0)}");
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

                Console.WriteLine("Want to listen cutted part?");
                Console.WriteLine("1. Yes");
                Console.WriteLine("2. No");
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

            using (var reader = new Mp3FileReader(inputPath))
            using (var writer = File.Create(outputPath))
            {
                Mp3Frame frame;
                while ((frame = reader.ReadNextFrame()) != null)
                {
                    switch (ch)
                    {
                        case "1":
                            if (reader.CurrentTime <= begin || !begin.HasValue)
                            {
                                if (reader.CurrentTime >= end || !end.HasValue)
                                    writer.Write(frame.RawData, 0, frame.RawData.Length);
                                else break;
                            }
                            break;

                        case "2":
                            if (reader.CurrentTime >= begin || !begin.HasValue)
                            {
                                if (reader.CurrentTime <= end || !end.HasValue)
                                    writer.Write(frame.RawData, 0, frame.RawData.Length);
                                else break;
                            }
                            break;
                    }
                }
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


        /*public static IWaveProvider CutAudio(WaveStream wave,
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
                Take = TimeSpan.Zero
            };

            wave.Position = currentPosition; // Restore stream position
            return (offset1.FollowedBy(offset2)).ToWaveProvider();

        } */

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
            Audio_info trackNew = new Audio_info(fileName, readerF.TotalTime.Minutes * 60 + readerF.TotalTime.Seconds);
            trackNew.info_track();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string sourceFile = "C:\\Users\\klausreinherz\\Downloads\\test.mp3";
            var reader = new AudioFileReader(sourceFile);
            var fileName = Path.GetFileName(sourceFile);

            Audio_info track1 = new Audio_info(fileName, reader.TotalTime.Minutes * 60 + reader.TotalTime.Seconds);
            track1.info_track();

            var waveOut = new WaveOutEvent();
            waveOut.Init(reader);

            Console.WriteLine("1. Play track");
            Console.WriteLine("2. Pause track");
            Console.WriteLine("1.1. Cut track");
            Console.WriteLine("0. Finish cut");
            
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
                        Console.WriteLine("1. Cut a part of the original track");
                        Console.WriteLine("2. Cut a part as new track");
                        var ch = Console.ReadLine();
                        Console.WriteLine("Start?");
                        var s = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("End?");
                        var e = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("Track name");
                        var n = Console.ReadLine();
                        string outputFile = $"C:\\Users\\klausreinherz\\Downloads\\{n}.mp3";
                        Console.WriteLine($"New duration: {track1.cut_track(sourceFile, outputFile, s, e, ch)} sec.");
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

            Console.WriteLine("Finish project");
            Console.ReadKey();
        }
    }
}

//2 глава должна быть готова к 12(20) апреля (печатать), список литературы тоже дожен быть
//курсовая работа должна быть готова к 18 мая
// 10 мая можно сдать раньше
// титульный лист
// введение - 1-2 страницы
// обязательно лист содержания
// главы - 4-6 страниц
// заключение - 1 страница
// список литературы(по госту), количество источников - не менее 3, в главах ссылка на каждый источник
