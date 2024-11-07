using System.Drawing;

namespace HillClimb
{
    public partial class Form1 : Form
    {

        private CancellationTokenSource _cts;
        private TaskCompletionSource<bool> _pauseTcs;
        private bool _isPaused;
        private bool _stepMode;

        public Dictionary<string, List<string>> regionGraph = new Dictionary<string, List<string>>
        {
            { "chernigiv_node", new List<string> { "kyiv_node", "poltava_node", "sumy_node" } },
            { "sumy_node", new List<string> { "kharkiv_node", "poltava_node", "chernigiv_node" } },
            { "kharkiv_node", new List<string> { "sumy_node", "poltava_node", "donetsk_node", "lugansk_node", "dnipro_node" } },
            { "donetsk_node", new List<string> { "lugansk_node", "kharkiv_node", "dnipro_node", "zaporizhzhya_node" } },
            { "lugansk_node", new List<string> { "kharkiv_node", "donetsk_node" } },
            { "zaporizhzhya_node", new List<string> { "donetsk_node", "dnipro_node", "kherson_node" } },
            { "dnipro_node", new List<string> { "poltava_node", "kharkiv_node", "donetsk_node", "zaporizhzhya_node", "kherson_node", "mykolaiv_node", "kirovograd_node" } },
            { "poltava_node", new List<string> { "kyiv_node", "chernigiv_node", "sumy_node", "kharkiv_node", "dnipro_node", "kirovograd_node", "cherkasy_node" } },
            { "kyiv_node", new List<string> { "chernigiv_node", "poltava_node", "cherkasy_node", "vinnitsya_node", "zhytomyr_node" } },
            { "cherkasy_node", new List<string> { "kyiv_node", "poltava_node", "kirovograd_node", "vinnitsya_node" } },
            { "kirovograd_node", new List<string> { "cherkasy_node", "poltava_node", "dnipro_node", "mykolaiv_node", "odesa_node", "vinnitsya_node" } },
            { "kherson_node", new List<string> { "zaporizhzhya_node", "krym_node", "mykolaiv_node", "dnipro_node" } },
            { "mykolaiv_node", new List<string> { "kherson_node", "dnipro_node", "kirovograd_node", "odesa_node" } },
            { "krym_node", new List<string> { "kherson_node" } },
            { "odesa_node", new List<string> { "mykolaiv_node", "kirovograd_node", "vinnitsya_node" } },
            { "vinnitsya_node", new List<string> { "chernivty_node", "odesa_node", "kirovograd_node", "cherkasy_node", "kyiv_node", "zhytomyr_node", "chmelnytsky_node" } },
            { "zhytomyr_node", new List<string> { "kyiv_node", "vinnitsya_node", "chmelnytsky_node", "rivne_node" } },
            { "rivne_node", new List<string> { "zhytomyr_node", "chmelnytsky_node", "lviv_node", "lutsk_node", "ternopil_node" } },
            { "lutsk_node", new List<string> { "rivne_node", "lviv_node" } },
            { "chmelnytsky_node", new List<string> { "chernivty_node", "ternopil_node", "rivne_node", "zhytomyr_node", "vinnitsya_node" } },
            { "chernivty_node", new List<string> { "vinnitsya_node", "chmelnytsky_node", "ternopil_node", "ivano_node" } },
            { "ivano_node", new List<string> { "chernivty_node", "ternopil_node", "lviv_node", "zakarpattya_node" } },
            { "zakarpattya_node", new List<string> { "ivano_node", "lviv_node" } },
            { "lviv_node", new List<string> { "ivano_node", "ternopil_node", "lutsk_node", "rivne_node", "zakarpattya_node" } },
            { "ternopil_node", new List<string> { "lviv_node", "rivne_node", "ivano_node", "chernivty_node", "chmelnytsky_node" } }
        };

        public Dictionary<string, Panel> panelMap;
        public Form1()
        {
            InitializeComponent();
            InitializePanelMap();
            _cts = new CancellationTokenSource();
            _pauseTcs = new TaskCompletionSource<bool>();
            _isPaused = false;
            _stepMode = false;
        }

        public void Form1_Load(object sender, EventArgs e)
        {

        }

        public void button1_Click(object sender, EventArgs e)
        {
            HillClimbingColoring();
        }

        public async void button2_Click(object sender, EventArgs e)
        {
            await Task.Run(() => Backtrack());
        }
        public void button3_Click(object sender, EventArgs e)
        {
            Random rand = new Random();
            foreach (var panel in panelMap.Values)
            {
                int colorIndex = rand.Next(colorOptions.Count);
                panel.BackColor = Color.FromName(colorOptions[colorIndex]);
            }
            regionColors.Clear();

            label2.Text = $"{ConflictCount()}";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            regionColors.Clear();

            // Установлюємо колір панелей у початковий стан
            foreach (var panel in panelMap.Values)
            {
                panel.BackColor = Color.Empty;
            }

            
            label2.Text = "0";
            
        }

        private void next_step_button_Click(object sender, EventArgs e)
        {
            _isPaused = false;
            _stepMode = true;
            _pauseTcs.TrySetResult(true);
        }

        private void pause_button_Click(object sender, EventArgs e)
        {
            _isPaused = true;
            _pauseTcs = new TaskCompletionSource<bool>();
        }

        private void play_button_Click(object sender, EventArgs e)
        {
            _isPaused = false;
            _stepMode = false;
            _pauseTcs.TrySetResult(true);
        }

        public void label2_Click(object sender, EventArgs e)
        {

        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _cts.Cancel();
            base.OnFormClosing(e);
        }

        public void InitializePanelMap()
        {
            panelMap = new Dictionary<string, Panel>
            {
                { "kharkiv_node", kharkiv_node },
                { "lugansk_node", lugansk_node },
                { "donetsk_node", donetsk_node },
                { "zaporizhzhya_node", zaporizhzhya_node },
                { "dnipro_node", dnipro_node },
                { "kherson_node", kherson_node },
                { "krym_node", krym_node },
                { "sumy_node", sumy_node },
                { "chernigiv_node", chernigiv_node },
                { "poltava_node", poltava_node },
                { "mykolaiv_node", mykolaiv_node },
                { "kirovograd_node", kirovograd_node },
                { "cherkasy_node", cherkasy_node },
                { "kyiv_node", kyiv_node },
                { "zhytomyr_node", zhytomyr_node },
                { "odesa_node", odesa_node },
                { "vinnitsya_node", vinnitsya_node },
                { "chmelnytsky_node", chmelnytsky_node },
                { "ternopil_node", ternopil_node },
                { "rivne_node", rivne_node },
                { "lutsk_node", lutsk_node },
                { "ivano_node", ivano_node },
                { "zakarpattya_node", zakarpattya_node },
                { "chernivty_node", chernivty_node },
                { "lviv_node", lviv_node },
            };
        }

        public Dictionary<string, string> regionColors = new Dictionary<string, string>();
        public List<string> colorOptions = new List<string> { "red", "green", "blue", "yellow"};

        public int iterationCount = 0;
        public int deadlockCount = 0;
        public int totalNodes = 0;
        public int nodesInMemory = 0;
        public async void HillClimbingColoring()
        {

            iterationCount = 0;
            deadlockCount = 0;
            totalNodes = regionGraph.Count;
            nodesInMemory = 0;
            //поки не буде все замальовано
            while (regionColors.Count < regionGraph.Count && !_cts.Token.IsCancellationRequested)
            {
                iterationCount++;
                await _pauseTcs.Task;
                if (_cts.Token.IsCancellationRequested) break;
                //знаходим мрв вузел
                var mrvRegion = regionGraph.Keys
                    .Where(region => !regionColors.ContainsKey(region))  // Только незамальовані вершини
                    .OrderBy(region => GetAvailableColors(region).Count) // Сортуєм по кількості доступних кольорів
                    .FirstOrDefault();

                //глухий кут
                if (mrvRegion == null || GetAvailableColors(mrvRegion).Count == 0)
                {
                    deadlockCount++;
                    break;
                }

                
                AssignColor(mrvRegion);

                
                if (panelMap.ContainsKey(mrvRegion))
                {
                    panelMap[mrvRegion].BackColor = Color.FromName(regionColors[mrvRegion]);
                    label2.Text = $"{ConflictCount()}";
                    iteration_label.Text = $"Iterations: {iterationCount}";
                    deadlock_label.Text = $"Deadlock count: {deadlockCount}";
                    totalNodes_label.Text = $"Total nodes: {totalNodes}";
                    nodesinmemory_label.Text = $"Nodes in memory: {regionColors.Count}";
                    await Task.Delay(150);
                }

                if (_stepMode)
                {
                    _isPaused = true;
                    _pauseTcs = new TaskCompletionSource<bool>();
                }
            }
        }

        
        private List<string> GetAvailableColors(string region)
        {
            var neighborColors = new HashSet<string>();
            foreach (var neighbor in regionGraph[region])
            {
                if (regionColors.ContainsKey(neighbor))
                {
                    neighborColors.Add(regionColors[neighbor]);
                }
            }
            return colorOptions.Where(color => !neighborColors.Contains(color)).ToList();
        }

        public int ConflictCount()
        {
            int conflictCount = 0;
            foreach (var region in regionGraph.Keys)
            {
                if (panelMap.ContainsKey(region))
                {
                    var regionColor = panelMap[region].BackColor;
                    foreach (var neighbor in regionGraph[region])
                    {
                        if (panelMap.ContainsKey(neighbor))
                        {
                            var neighborColor = panelMap[neighbor].BackColor;
                            
                            if (regionColor == neighborColor)
                            {
                                conflictCount++;
                            }
                        }
                    }
                }
            }
            
            return conflictCount / 2;
        }

        public void AssignColor(string region)
        {
            
            var neighborColors = new HashSet<string>();
            foreach (var neighbor in regionGraph[region])
            {
                if (regionColors.ContainsKey(neighbor))
                {
                    neighborColors.Add(regionColors[neighbor]);
                }
            }

            
            foreach (var color in colorOptions)
            {
                if (!neighborColors.Contains(color))
                {
                    regionColors[region] = color;
                    break;
                }
            }
        }

        private int backtrackIterationCount = 0;
        private int backtrackDeadlockCount = 0;
        private int restartCount = 0;
        private const int maxRestarts = 5;
        private async Task<bool> Backtrack()
        {
            iterationCount = 0;
            deadlockCount = 0;
            totalNodes = regionGraph.Count;
            nodesInMemory = 0;
            backtrackIterationCount = 0;
            backtrackDeadlockCount = 0;
            restartCount = 0;
            _pauseTcs = new TaskCompletionSource<bool>();

            
            return await BacktrackRecursive();
        }

        // Рекурсивна функція для чистого бектрекінгу
        private async Task<bool> BacktrackRecursive()
        {
            iterationCount++;

            
            if (AllRegionsColored())
            {
                UpdateLabels();
                return true;
            }

            await _pauseTcs.Task;
            if (_cts.Token.IsCancellationRequested) return false;

            var Region = regionGraph.Keys.FirstOrDefault(region => !regionColors.ContainsKey(region));

            //if (iterationCount % 2 == 0) // кожна друга ітерація використовує MRV
            //{
            //    mrvRegion = regionGraph.Keys
            //        .Where(region => !regionColors.ContainsKey(region))
            //        .OrderBy(region => GetAvailableColors(region).Count)
            //        .FirstOrDefault();
            //}
            //else
            //{
            //    mrvRegion = regionGraph.Keys.FirstOrDefault(region => !regionColors.ContainsKey(region));
            //}


            if (Region == null)
            {
                
                UpdateLabels();
                return false;
            }

            
            foreach (var color in GetAvailableColors(Region).OrderBy(c => Guid.NewGuid()))
            {
                if (IsSafeToColor(Region, color))
                {
                    regionColors[Region] = color;

                    
                    if (panelMap.ContainsKey(Region))
                    {
                        this.Invoke((MethodInvoker)(() =>
                            panelMap[Region].BackColor = Color.FromName(color)
                        ));

                        
                        await Task.Delay(150);

                    }
                    if (_stepMode)
                    {
                        _isPaused = true;
                        _pauseTcs = new TaskCompletionSource<bool>();
                    }
                    UpdateLabels();
                    
                    if (await BacktrackRecursive())
                    {
                        return true;
                    }

                    
                    regionColors.Remove(Region);
                    deadlockCount++;

                    if (panelMap.ContainsKey(Region))
                    {
                        this.Invoke((MethodInvoker)(() =>
                            panelMap[Region].BackColor = Color.Empty
                        ));

                        
                        await Task.Delay(150);
                    }
                }
            }

            return false;
        }

        private void UpdateLabels()
        {
            
            this.Invoke((MethodInvoker)(() =>
            {
                label2.Text = $"{ConflictCount()}";
                iteration_label.Text = $"Iterations: {iterationCount}";
                deadlock_label.Text = $"Deadlock count: {deadlockCount}";
                totalNodes_label.Text = $"Total nodes: {totalNodes}";
                nodesinmemory_label.Text = $"Nodes in memory: {regionColors.Count}";
                restart_label.Text = $"Restarts: {restartCount}/{maxRestarts}";
            }));
        }



        private bool AllRegionsColored()
        {
            return regionColors.Count == regionGraph.Count;
        }

        private bool IsSafeToColor(string region, string color)
        {
            foreach (var neighbor in regionGraph[region])
            {
                if (regionColors.ContainsKey(neighbor) && regionColors[neighbor] == color)
                {
                    return false;
                }
            }
            return true;
        }

        
    }
}
