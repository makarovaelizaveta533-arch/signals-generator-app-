using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SignalGenerator.App
{
    // partial class = класс разрезан на 2 файла: этот (.cs) + MainWindow.xaml
    // : Window = наше окно является окном WPF
    public partial class MainWindow : Window
    {
        // List = список. SingleChannelSignal = один сигнал.
        // signalList хранит ВСЕ сигналы, которые уже добавили на график
        private List<SingleChannelSignal> signalList = new List<SingleChannelSignal>();

        // Список зашкалов. (int start, int end) = пара чисел: начало и конец куска
        // Это индексы точек в массиве, НЕ секунды
        private List<(int start, int end)> outOfRangeMarkers = new List<(int, int)>();

        // Конструктор = вызывается автоматически, когда окно создаётся
        public MainWindow()
        {
            // InitializeComponent строит интерфейс из MainWindow.xaml
            InitializeComponent();
        }

        // addSignal = наша вспомогательная функция: сохранить сигнал + нарисовать
        private void addSignal(SingleChannelSignal signal)
        {
            signalList.Add(signal); // Add = добавить элемент в КОНЕЦ списка
            // ScatterLine = нарисовать линию по точкам. X=время, Y=значения
            WpfPlot1.Plot.Add.ScatterLine(signal.TimeTick, signal.Value);
            WpfPlot1.Plot.Axes.AutoScale(); // AutoScale = подогнать оси под данные
            WpfPlot1.Refresh();             // Refresh = перерисовать картинку на экране
        }

        // Этот метод вызывается, когда нажали кнопку Sin (см. Click в XAML)
        // object sender = кто нажал (кнопка). RoutedEventArgs e = доп.инфо о событии (нам не важно)
        private void Button_Click_Sin(object sender, RoutedEventArgs e)
        {
            // float = тип «число с точкой» (20, 0.5, -1.2)
            // len = имя переменной (длина сигнала в секундах)
            // float.Parse(...) = превратить ТЕКСТ в ЧИСЛО
            // len_textBox.Text = то, что сейчас написано в поле на экране (это строка!)
            // CultureInfo.InvariantCulture = читать точку как десятичный знак (0.5), а не зависеть от Windows
            float len = float.Parse(len_textBox.Text, CultureInfo.InvariantCulture);

            // То же самое: читаем частоту из поля freq_textBox и делаем из текста число
            float freq = float.Parse(freq_textBox.Text, CultureInfo.InvariantCulture);

            // Читаем период синуса из period_textBox → число
            float period = float.Parse(period_textBox.Text, CultureInfo.InvariantCulture);

            // Читаем фазу (сдвиг) из phase_textBox → число
            float phase = float.Parse(phase_textBox.Text, CultureInfo.InvariantCulture);

            // Читаем масштаб (высоту волны) из scale_textBox → число
            float scale = float.Parse(scale_textBox.Text, CultureInfo.InvariantCulture);

            // Tools.Sin(...) = создать синус. addSignal(...) = добавить на график
            addSignal(Tools.Sin(len, freq, period, phase, scale));
        }

        // Метод кнопки "Прямая"
        private void Button_Click_Line(object sender, RoutedEventArgs e)
        {
            // float.Parse = текст из поля → число. len = длина в секундах
            float len = float.Parse(len_textBox.Text, CultureInfo.InvariantCulture);
            // freq = частота дискретизации (сколько точек в секунду)
            float freq = float.Parse(freq_textBox.Text, CultureInfo.InvariantCulture);
            // k = наклон прямой (y = k*t + b)
            float k = float.Parse(k_textBox.Text, CultureInfo.InvariantCulture);
            // b = сдвиг прямой вверх/вниз
            float b = float.Parse(b_textBox.Text, CultureInfo.InvariantCulture);
            // Tools.Line = сгенерировать прямую, addSignal = нарисовать
            addSignal(Tools.Line(len, freq, k, b));
        }

        // Метод кнопки "Шум"
        private void Button_Click_Noise(object sender, RoutedEventArgs e)
        {
            // Текст из поля длины → число float
            float len = float.Parse(len_textBox.Text, CultureInfo.InvariantCulture);
            // Текст из поля частоты → число float
            float freq = float.Parse(freq_textBox.Text, CultureInfo.InvariantCulture);
            // Текст из поля амплитуды → число float (насколько сильный шум)
            float amplitude = float.Parse(amplitude_textBox.Text, CultureInfo.InvariantCulture);
            // Tools.Noise = случайный шум, addSignal = нарисовать
            addSignal(Tools.Noise(len, freq, amplitude));
        }

        // Метод кнопки "Удалить"
        private void Button_Delete(object sender, RoutedEventArgs e)
        {
            // if = если. Count = сколько элементов в списке. == 0 значит пусто
            // return = сразу выйти из метода, дальше код не выполнять
            if (signalList.Count == 0) return;

            // RemoveAt = удалить элемент по номеру. Count-1 = последний элемент
            signalList.RemoveAt(signalList.Count - 1);

            // PlottableList = список всего, что нарисовано на графике
            var list = WpfPlot1.Plot.PlottableList;

            // Если на графике что-то есть — удалить последнюю нарисованную штуку
            if (list.Count > 0)
                WpfPlot1.Plot.Remove(list[list.Count - 1]);

            WpfPlot1.Plot.Axes.AutoScale(); // подогнать оси заново
            WpfPlot1.Refresh();             // обновить картинку
        }

        // Метод кнопки "Сумма"
        private void Button_Sum(object sender, RoutedEventArgs e)
        {
            // Tools.Summ складывает ВСЕ сигналы из signalList поточечно
            addSignal(Tools.Summ(signalList));
        }

        // Метод кнопки "Склейка"
        private void Button_Skley(object sender, RoutedEventArgs e)
        {
            // Tools.Skley ставит сигналы друг за другом (как вагоны)
            addSignal(Tools.Skley(signalList));
        }

        // Метод кнопки "Сброс"
        private void Button_Clear(object sender, RoutedEventArgs e)
        {
            signalList.Clear();          // Clear = очистить список сигналов полностью
            outOfRangeMarkers.Clear();   // очистить список зашкалов
            WpfPlot1.Plot.Clear();       // стереть всё с графика
            WpfPlot1.Refresh();          // обновить экран
        }

        // Метод кнопки "Загрузить" (загрузить СИГНАЛ из файла)
        private void Button_Load(object sender, RoutedEventArgs e)
        {
            // new = создать объект. OpenFileDialog = стандартное окно «Выберите файл»
            OpenFileDialog myDialog = new OpenFileDialog();
            // Filter = показывать только csv-файлы
            myDialog.Filter = "csv-файл(*.csv)|*.csv";
            // CheckFileExists = нельзя выбрать несуществующий файл
            myDialog.CheckFileExists = true;
            // Multiselect = false = выбрать можно только один файл
            myDialog.Multiselect = false;
            // ShowDialog показывает окно. == true значит пользователь нажал «Открыть»
            if (myDialog.ShowDialog() == true)
                // FileName = путь к выбранному файлу. SignalReader читает csv → сигнал
                addSignal(SingleChannelSignalReader.SignalReader(myDialog.FileName));
        }

        // Метод кнопки "Сохранить" (сохранить СИГНАЛ в файл)
        private void Button_Save(object sender, RoutedEventArgs e)
        {
            // Если сигналов нет — выходим
            if (signalList.Count == 0) return;

            // SaveFileDialog = окно «Куда сохранить файл»
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "csv-файл (*.csv)|*.csv"; // только csv
            saveFileDialog.FileName = "MySignal";             // имя по умолчанию
            saveFileDialog.DefaultExt = "csv";                // расширение .csv

            // Если нажали «Сохранить»
            if (saveFileDialog.ShowDialog() == true)
                // Берём ПОСЛЕДНИЙ сигнал (Count-1) и пишем его в файл
                SingleChannelSignalWriter.SignalWriter(signalList[signalList.Count - 1], saveFileDialog.FileName);
        }

        // Метод кнопки "Фильтр офлайн"
        private void Button_FilterOffline(object sender, RoutedEventArgs e)
        {
            // Нет сигналов — выходим
            if (signalList.Count == 0) return;

            // Freq = частота последнего сигнала в списке
            float freq = signalList[signalList.Count - 1].Freq;

            // Читаем размер окна из текстового поля (в СЕКУНДАХ) → число float
            // float.Parse = текст → число. CultureInfo.InvariantCulture = точка как 0.5
            float windowSec = float.Parse(window_textBox.Text, CultureInfo.InvariantCulture);

            // Переводим секунды в количество точек: секунды * частота
            // (int) = отбросить дробную часть. +0.5 = чтобы округлить «по-честному»
            int windowSize = (int)(windowSec * freq + 0.5);

            // % = остаток от деления. Если чётное (остаток 0) — делаем нечётным (+1)
            // Нечётное окно нужно для симметричного офлайн-фильтра
            if (windowSize % 2 == 0) windowSize++;

            // Value = массив чисел сигнала. OfflineFilter.Filter = сгладить весь массив
            float[] filtered = OfflineFilter.Filter(signalList[signalList.Count - 1].Value, windowSize);

            // new SingleChannelSignal = новый сигнал из частоты + отфильтрованных чисел
            addSignal(new SingleChannelSignal(freq, filtered));
        }

        // Метод кнопки "Фильтр онлайн"
        private void Button_FilterOnline(object sender, RoutedEventArgs e)
        {
            // Нет сигналов — выходим
            if (signalList.Count == 0) return;

            // Частота последнего сигнала
            float freq = signalList[signalList.Count - 1].Freq;

            // Текст окна (секунды) → число float
            float windowSec = float.Parse(window_textBox.Text, CultureInfo.InvariantCulture);

            // Секунды → количество точек. +0.5 для округления
            int windowSize = (int)(windowSec * freq + 0.5);

            // Если чётное — сделать нечётным
            if (windowSize % 2 == 0) windowSize++;

            // data = массив значений последнего сигнала
            float[] data = signalList[signalList.Count - 1].Value;

            // new OnlineFilter = создать онлайн-фильтр с буфером на windowSize точек
            OnlineFilter onlineFilter = new OnlineFilter(windowSize);

            // chunkSize = размер куска. Будем давать данные по 7 точек (как в задании)
            int chunkSize = 7;

            // result = список, куда складываем отфильтрованные числа
            List<float> result = new List<float>();

            // for = цикл. i начинается с 0, пока i < длины, каждый раз i увеличивается на 7
            for (int i = 0; i < data.Length; i += chunkSize)
            {
                // Math.Min = меньшее из двух чисел. В конце сигнала кусок может быть короче 7
                int actualSize = Math.Min(chunkSize, data.Length - i);

                // chunk = маленький массив на actualSize чисел
                float[] chunk = new float[actualSize];

                // Копируем кусок из большого массива data в маленький chunk
                for (int j = 0; j < actualSize; j++)
                    chunk[j] = data[i + j];

                // Filter(chunk) = сгладить кусок. AddRange = дописать все числа в result
                result.AddRange(onlineFilter.Filter(chunk));
            }

            // ToArray = список → обычный массив. Потом новый сигнал и на график
            addSignal(new SingleChannelSignal(freq, result.ToArray()));
        }

        // Метод кнопки "Зашкалы"
        private void Button_Analyze(object sender, RoutedEventArgs e)
        {
            // Нет сигналов — выходим
            if (signalList.Count == 0) return;

            // Частота последнего сигнала (чтобы индекс точки перевести в секунды)
            float freq = signalList[signalList.Count - 1].Freq;

            // Текст минимума → число float
            float min = float.Parse(min_textBox.Text, CultureInfo.InvariantCulture);

            // Текст максимума → число float
            float max = float.Parse(max_textBox.Text, CultureInfo.InvariantCulture);

            // Минимальная длина зашкала в точках. 1 = даже одна точка считается
            int minDuration = 1;

            // Очистить старые маркеры зашкалов
            outOfRangeMarkers.Clear();

            // Создать анализатор с нашими границами min/max
            var analyzer = new SingleChannelRangeAnalyzer(min, max, minDuration);

            // += подписка на событие. Когда анализатор найдёт зашкал — выполнится код в { }
            // (s, args) => { ... } = короткая функция прямо здесь (лямбда)
            // s = кто послал событие, args = данные (StartIndex, EndIndex)
            analyzer.OutOfRangeDetected += (s, args) =>
            {
                // Добавить пару (начало, конец) в список маркеров
                outOfRangeMarkers.Add((args.StartIndex, args.EndIndex));

                // Индекс точки / частота = время в секундах (для оси X графика)
                float tStart = args.StartIndex / freq;
                float tEnd = args.EndIndex / freq;

                // VerticalLine = вертикальная красная линия на графике
                // #FF0000 = красный цвет в hex
                WpfPlot1.Plot.Add.VerticalLine(tStart, 2, ScottPlot.Color.FromHex("#FF0000"));
                WpfPlot1.Plot.Add.VerticalLine(tEnd, 2, ScottPlot.Color.FromHex("#FF0000"));
            };

            // Запустить анализ массива значений последнего сигнала
            analyzer.Analyze(signalList[signalList.Count - 1].Value);

            WpfPlot1.Plot.Axes.AutoScale(); // подогнать оси
            WpfPlot1.Refresh();             // обновить график

            // MessageBox.Show = всплывающее окошко с текстом
            // $ ste строки = можно вставить {переменную} прямо в текст
            MessageBox.Show($"Найдено выходов за диапазон: {outOfRangeMarkers.Count}", "Анализ");
        }

        // Пустой метод. Visual Studio иногда создаёт такое сама. Ничего не делает.
        private void freq_textBox_TextChanged(object sender, TextChangedEventArgs e) { }

        // Метод кнопки "Сохранить разметку" (разметка = индексы зашкалов, НЕ XAML)
        private void Button_SaveMarkers(object sender, RoutedEventArgs e)
        {
            // Если маркеров нет — показать сообщение и выйти
            if (outOfRangeMarkers.Count == 0)
            {
                MessageBox.Show("Нет разметки для сохранения.", "Сохранение");
                return;
            }

            // Окно «куда сохранить»
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "csv-файл (*.csv)|*.csv";
            saveFileDialog.FileName = "Markers";
            saveFileDialog.DefaultExt = "csv";

            // Если нажали «Сохранить»
            if (saveFileDialog.ShowDialog() == true)
            {
                // using = открыли файл, а в конце блока он сам закроется
                // StreamWriter = писатель текстового файла
                using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                {
                    // foreach = для каждого элемента списка
                    foreach (var marker in outOfRangeMarkers)
                        // WriteLine = записать строку и перейти на новую. Формат: 15;42
                        writer.WriteLine($"{marker.start};{marker.end}");
                }
            }
        }

        // Метод кнопки "Загрузить разметку"
        private void Button_LoadMarkers(object sender, RoutedEventArgs e)
        {
            // Без сигнала некуда рисовать линии — предупреждаем и выходим
            if (signalList.Count == 0)
            {
                MessageBox.Show("Сначала загрузите сигнал.", "Загрузка");
                return;
            }

            // Окно выбора файла
            OpenFileDialog myDialog = new OpenFileDialog();
            myDialog.Filter = "csv-файл(*.csv)|*.csv";
            myDialog.CheckFileExists = true;
            myDialog.Multiselect = false;

            // Если нажали «Открыть»
            if (myDialog.ShowDialog() == true)
            {
                // Очистить старые маркеры
                outOfRangeMarkers.Clear();

                // Частота нужна, чтобы индекс → секунды
                float freq = signalList[signalList.Count - 1].Freq;

                // StreamReader = читатель текстового файла. using сам закроет файл
                using (StreamReader reader = new StreamReader(myDialog.FileName))
                {
                    // Бесконечный цикл, выходим через break
                    while (true)
                    {
                        // ReadLine = прочитать одну строку. null = файл закончился
                        string line = reader.ReadLine();
                        if (line == null) break;

                        // Split(';') = разрезать строку "15;42" на массив ["15","42"]
                        string[] parts = line.Split(';');

                        // int.Parse = текст → целое число. parts[0] = начало, parts[1] = конец
                        int start = int.Parse(parts[0]);
                        int end = int.Parse(parts[1]);

                        // Сохранить маркер в список
                        outOfRangeMarkers.Add((start, end));

                        // Индекс / частота = время в секундах
                        float tStart = start / freq;
                        float tEnd = end / freq;

                        // Нарисовать две красные вертикальные линии
                        WpfPlot1.Plot.Add.VerticalLine(tStart, 2, ScottPlot.Color.FromHex("#FF0000"));
                        WpfPlot1.Plot.Add.VerticalLine(tEnd, 2, ScottPlot.Color.FromHex("#FF0000"));
                    }
                }

                WpfPlot1.Plot.Axes.AutoScale();
                WpfPlot1.Refresh();
                MessageBox.Show($"Загружено разметок: {outOfRangeMarkers.Count}", "Загрузка");
            }
        }
    }
}
