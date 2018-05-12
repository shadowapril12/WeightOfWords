using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HomeWeight
{
    class Program
    {
        static void Main(string[] args)
        {
            ///Массив из документов
            var lines = new[] {
                "Тегеран будет сопротивляться решению Вашингтона выйти из иранской ядерной сделки и не продлевать снятие санкций с Исламской Республики, если оно будет принято. Об этом сказал президент Ирана Хасан Рухани, передает Reuters.",
                "Если Америка покинет ядерное соглашение, она пожалеет об этом так, как не жалела никогда в истории, — заявил Рухани. — Наша организация по атомной энергетике и экономический сектор будут противостоять американским козням против нашей страны.",
                "Высокопоставленный иранский чиновник сообщил Reuters, что выход США из соглашения сделает Рухани политически уязвимым и «повредит его легитимности за рубежом». Отказ Трампа от условий сделки означает для иранского президента сложности в развитии отношений с Западом, полагает собеседник агентства."
            };

            ///Получение коллекции словарей, сосотоящих из пар ключ - значение, где ключ - слово, а значение - вес этого слова в
            ///рассматриваемом документе
            List<Dictionary<string, double>> l = GetTfidf(lines);

            Console.ReadLine();
        }

        /// <summary>
        /// Метод GetTfidf предназначен для рассчета значимости слова для каждого документа
        /// </summary>
        /// <param name="lines">Массив документов</param>
        /// <returns>Возвращает коллекцию словарей, сосотоящих из пар ключ - значение, где ключ - слово, а значение - вес этого слова
        /// в рассматриваемом документе</returns>
        private static List<Dictionary<string, double>> GetTfidf(string[] lines)
        {
            var regex = new Regex("\\w+", RegexOptions.Compiled);
            var data = lines
                .Select(line => regex.Matches(line)
                    .OfType<Match>()
                    .Select(m => m.Value)
                    //.Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.ToLower())
                    .ToArray())
                .ToArray();

            var words = data.SelectMany(d => d).Distinct().OrderBy(s => s).ToArray();

            //Коллекция vocabulary состоит из пар ключ - значение, где ключ слово, а значимость слова - значение
            Dictionary<string, double> vocabulary = new Dictionary<string, double>();

            //Коллекция из словарей vcabulary, для каждого документа
            List<Dictionary<string, double>> weightList = new List<Dictionary<string, double>>();

            for(int i = 0; i < lines.Length; i++)
            {
                for(int k = 0; k < words.Length; k++)
                {
                    //Получаем значение TF
                    double TF = GetTF(words[k], lines[i]);

                    //Значение IDF
                    double IDF = GetIDF(words[k], lines);

                    //И значимость слова для документа
                    double weight = TF * IDF;

                    //Добавляем полученные значения в коллекцию vocabulary
                    vocabulary.Add(words[k], weight);
                }

                //Добавляем полученные vocabulary в коллекцию словарей
                weightList.Add(vocabulary);

                //Пересоздаем словарь
                vocabulary = new Dictionary<string, double>();                              
            }

            //Вывод значимостей всх слов для каждого документа
            for(int i = 0; i < weightList.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Значимость слов для документа {i}");
                Console.ResetColor();

                foreach(KeyValuePair<string, double> el in weightList[i])
                {
                    Console.WriteLine($"{el.Key} - {el.Value}");
                }
            }

            return weightList;
        }

        /// <summary>
        /// Метод GetTF предназначен для рассчета TF для каждоого слова в документе
        /// </summary>
        /// <param name="word">Слово для которого рассчитывается TF</param>
        /// <param name="text">Текст документа</param>
        /// <returns>TF</returns>
        private static double GetTF(string word, string text)
        {
            //Приведение текста документа к нижнему регистру
            text = text.ToLower();

            //Получение массива слов из документа
            string[] arr = text.Split(new[] { ' ', ',', ':', '?', '!', '«', '»', '-', '.' }, StringSplitOptions.RemoveEmptyEntries);

            //Количество слов 'word' в данном документе
            double wordRepeat = 0;

            //Если документ содержит рассматриваемое слово
            if(text.Contains(word))
            {
                //Получение количества повторений данного слова в тексте
                wordRepeat = arr.Select(p => p).Where(p => p == word).Count();
            }

            //Количество слов в документе
            int documentLength = arr.Length;         

            //Рассчет и возврат TF
            return wordRepeat / documentLength;
        }

        /// <summary>
        /// Метод GetIDF прдназначен для рассчета IDF для каждого слова в документе
        /// </summary>
        /// <param name="word">Рассматриваемое слово в документе</param>
        /// <param name="documents">Количество документов</param>
        /// <returns>IDF</returns>
        private static double GetIDF(string word, string[] documents)
        {
            //Количество документов в массиве
            int documentCount = documents.Length;

            //Счетчик количества документов содержащих данное слово
            int wordCount = 0;

            for(int i = 0; i < documents.Length; i++)
            {
                //Приведение текста документа к нижнему регистру
                documents[i] = documents[i].ToLower();

                //Если документ содержит слово
                if(documents[i].Contains(word))
                {
                    //Счетчик увеличивается на единицу
                    wordCount++;
                }
            }
            //Возвращается IDF
            return (double)wordCount / documents.Length;
        }
    }
}
