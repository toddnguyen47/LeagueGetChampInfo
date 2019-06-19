using LeagueChampName;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Text;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        #region Variable declaration
        public static String VERSION_NUMBER = "1.1.1";
        private const int MAX_RECENT_CHAMPS = 5;

        private List<String> allChampions = new List<String>();
        private String csvFile = "championNames.txt";
        private String jsonSettingsFile = "settings.json";

        private CustomListViewItem currentlySelectedLvi = null;

        private string[] m_siteNames = { "checkUGG", "checkOpGG", "checkLolalytics", "checkChampionGG" };
        private Dictionary<String, String> m_lolalyticsChampNames;

        private CustomListViewItem[] m_arrRecentlyPlayedChamps = new CustomListViewItem[5];
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            CustomInit();
        }

        private void CustomInit()
        {
            this.readInCsvFile();
            this.setupListView();
            this.readjsonSettingsFile();
            this.setupLolalyticsChampNames();
        }

        /// <summary>
        /// Read in CSV files.
        /// </summary>
        private void readInCsvFile()
        {
            if (false == System.IO.File.Exists(this.csvFile))
            {
                MessageBox.Show(String.Format("{0} does not exist. Please make sure " +
                    "{0} is in the same directory as this executable before running the " +
                    "program.", this.csvFile));
                // Exit the program
                System.Windows.Application.Current.Shutdown();
                return;
            }

            System.IO.StreamReader reader = new System.IO.StreamReader(this.csvFile);

            string currentLine;
            while ((currentLine = reader.ReadLine()) != null)
            {
                this.allChampions.Add(currentLine);
            }

            reader.Close();
        }

        
        private void SelectItemHandler(object sender, RoutedEventArgs e)
        {
            // Unselect the currently selected first
            if (this.currentlySelectedLvi != null)
                this.currentlySelectedLvi.IsSelected = false;

            CustomListViewItem selected = (CustomListViewItem)sender;
            this.currentlySelectedLvi = selected;
        }


        /// <summary>
        /// Setup the ListView
        /// </summary>
        private void setupListView()
        {
            // Setup the All Champions ListView
            foreach (String s in this.allChampions)
            {
                CustomListViewItem lvi = new CustomListViewItem();
                lvi.Selected += SelectItemHandler;
                lvi.Content = s;

                ChampionListView.Items.Add(lvi);
            }
        }


        /// <summary>
        /// Read in the JSON settings, if the JSON file exists.
        /// </summary>
        private void readjsonSettingsFile()
        {
            // Initialize index to 0
            int recentPlayedIndex = 0;

            // Initialize the length of the recent champs to 0
            int recentChampLen = 0;

            // Only read in JSON file if the file exists
            if (System.IO.File.Exists(this.jsonSettingsFile))
            {
                // Read in the JSON file first
                System.IO.StreamReader reader = new System.IO.StreamReader(this.jsonSettingsFile);
                string jsonString = reader.ReadToEnd();

                // Convert JSON String to a C# object
                SettingsJsonObject jsonStuff = JsonConvert.DeserializeObject <SettingsJsonObject> (jsonString);
                
                // Set the correct sites
                for (int i = 0, i2 = jsonStuff.sitesSelected.Length; i < i2; i++)
                {
                    bool boolSite = jsonStuff.sitesSelected[i];
                    ((CheckBox)this.FindName(this.m_siteNames[i])).IsChecked = boolSite;
                }


                // Set the most recent 5 champions
                mostRecentChamps.Items.Clear();
                recentChampLen = jsonStuff.fiveRecentChampions.Length;

                for (int i = 0, i2 = recentChampLen; i < i2; i++)
                {
                    String curChamp = jsonStuff.fiveRecentChampions[i];
                    CustomListViewItem lvi = new CustomListViewItem();
                    lvi.Content = curChamp;
                    lvi.Selected += SelectItemHandler;

                    this.mostRecentChamps.Items.Add(lvi);
                    this.m_arrRecentlyPlayedChamps[recentPlayedIndex++] = lvi;
                }

                reader.Close();
            }

            // If there are less than 5 champs, add in empty placeholders
            int diff = MAX_RECENT_CHAMPS - recentChampLen;
            for (int i = 0; i < diff; i++)
            {
                CustomListViewItem lvi = new CustomListViewItem();
                lvi.Focusable = false;
                lvi.Content = "";
                this.mostRecentChamps.Items.Add(lvi);
                this.m_arrRecentlyPlayedChamps[recentPlayedIndex++] = lvi;
            }
        }


        /// <summary>
        /// Setup the HashSet of champ names for lolalytics.
        /// The key will be the lowercase name of the champion with no special characters.
        /// The value will be the value that lolalytics expects
        /// </summary>
        private void setupLolalyticsChampNames()
        {
            this.m_lolalyticsChampNames = new Dictionary<String, String>();
            this.m_lolalyticsChampNames.Add("Chogath".ToLower(), "Chogath");
            this.m_lolalyticsChampNames.Add("Kaisa".ToLower(), "Kaisa");
            this.m_lolalyticsChampNames.Add("Khazix".ToLower(), "Khazix");
            this.m_lolalyticsChampNames.Add("KogMaw".ToLower(), "KogMaw");
            this.m_lolalyticsChampNames.Add("RekSai".ToLower(), "RekSai");
            this.m_lolalyticsChampNames.Add("Velkoz".ToLower(), "Velkoz");
        }

        public bool ChampNameFilter(String item)
        {
            String text1 = ChampSearchTextbox.Text;

            // If text is null
            if (String.IsNullOrEmpty(text1))
                return true;
            else
            {
                return item.ToUpper().Contains(text1.ToUpper());
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Current Version: " + MainWindow.VERSION_NUMBER, "Version Number");
        }

        private void ClearSavedItem_Click(object sender, RoutedEventArgs e)
        {
            // Ref: https://social.msdn.microsoft.com/Forums/vstudio/en-US/d3f223ac-7fca-486e-8939-adb46e9bf6c9/how-can-i-get-yesno-from-a-messagebox-in-wpf?forum=wpf
            MessageBoxResult result = MessageBox.Show("Reset all saved data?", "Reset", MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            // Only reset if user clicks yes
            if (result == MessageBoxResult.Yes)
            {
                this.resetRecentChampList();
                this.resetSiteToVisit();

                // Delete settings.json
                if (File.Exists(this.jsonSettingsFile))
                    File.Delete(this.jsonSettingsFile);
            }
        }


        /// <summary>
        /// Reset the recent champion list.
        /// </summary>
        private void resetRecentChampList()
        {
            for (int i = 0, i2 = this.m_arrRecentlyPlayedChamps.Length; i < i2; i++)
            {
                CustomListViewItem cvli = new CustomListViewItem();
                cvli.Content = "";
                cvli.Focusable = false;
                this.m_arrRecentlyPlayedChamps[i] = cvli;
            }

            this.refreshRecentChampList();
        }


        /// <summary>
        /// Reset the site to visit
        /// </summary>
        private void resetSiteToVisit()
        {
            // Uncheck all boxes but u.gg
            for (int i = 1, i2 = this.m_siteNames.Length; i < i2; i++)
            {
                ((CheckBox)this.FindName(this.m_siteNames[i])).IsChecked = false;
            }

            ((CheckBox)this.FindName(this.m_siteNames[0])).IsChecked = true;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Would you like to exit?", "Exit",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            // Only exit if user clicks yes
            if (result == MessageBoxResult.Yes)
            {
                // Ref: https://stackoverflow.com/a/2820377
                System.Windows.Application.Current.Shutdown();
                return;
            }
        }


        /// <summary>
        /// Whenever text changes in the search textbox, filter the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChampSearchTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += filterListViewAndRefresh;
            worker.RunWorkerAsync();
        }


        private void filterListViewAndRefresh(object sender, DoWorkEventArgs e)
        {
            // Ref: https://stackoverflow.com/a/9732853
            // We need to do this for multi threading
            this.Dispatcher.Invoke(() =>
            {
                // Check for selected index
                //if (String.IsNullOrEmpty(this.currentlySelected))

                List<CustomListViewItem> matches = new List<CustomListViewItem>();
                int count = this.allChampions.Count;

                // Get all items in the ListBox
                for (int i = 0, i2 = count; i < i2; i++)
                {
                    String s = this.allChampions[i];
                    // If name matches
                    if (this.ChampNameFilter(s))
                    {
                        CustomListViewItem lvi = new CustomListViewItem();
                        lvi.Content = s;
                        lvi.Selected += SelectItemHandler;

                        matches.Add(lvi);
                    }
                }

                ChampionListView.Items.Clear();

                // Re-add back in
                foreach (CustomListViewItem lvi in matches)
                {
                    ChampionListView.Items.Add(lvi);

                    if (lvi == this.currentlySelectedLvi)
                    {
                        ChampionListView.SelectedItem = lvi;
                    }
                }
            });
        }


        /// <summary>
        /// Lowercase all characters, except the first character. The first character will be uppercase.
        /// </summary>
        /// <param name="input">String input</param>
        /// <returns>Converted String</returns>
        public String ToLowerExceptFirstChar(String input)
        {
            StringBuilder sb = new StringBuilder();

            // Uppercase first character
            sb.Append(Char.ToUpper(input[0]));

            // Lowercase the rest
            for (int i = 1, i2 = input.Length; i < i2; i++)
            {
                char curChar = input[i];
                sb.Append(Char.ToLower(curChar));
            }

            return sb.ToString();
        }


        /// <summary>
        /// Handle what happens when the user clicks on the "Find Builds" button.
        /// </summary>
        private void FindBuildsBtn_Click(object sender, RoutedEventArgs e)
        {
            // If nothing is selected
            if (this.currentlySelectedLvi == null)
                MessageBox.Show("No champion is selected yet!");
            else
            {
                // Start the website
                int idListLength = this.m_siteNames.Length;

                // Get a RegEx pattern: delete all characters that is not a-z, A-Z, or 0-9
                string pattern = "[^a-zA-Z0-9]";

                for (int i = 0; i < idListLength; i++)
                {
                    String id1 = this.m_siteNames[i];
                    var elem = (CheckBox)this.FindName(id1);
                    bool isChecked = (bool)elem.IsChecked;

                    if (isChecked)
                    {
                        String url1 = "";
                        String formattedChamp = this.currentlySelectedLvi.Content.ToString();

                        // Special cases!
                        // Nunu
                        if (formattedChamp.Equals("Nunu & Willump")) formattedChamp = "Nunu";

                        // Replace using regex pattern:
                        // delete all characters that is not a-z, A-Z, or 0-9
                        formattedChamp = Regex.Replace(formattedChamp, pattern, "");

                        // Uppercase first character, lowercase the rest
                        formattedChamp = this.ToLowerExceptFirstChar(formattedChamp);

                        switch (i)
                        {
                            // If it is u.gg
                            case 0:
                                url1 = String.Format("https://u.gg/lol/champions/{0}/build",
                                formattedChamp);
                                break;
                            // op.gg
                            case 1:
                                url1 = String.Format("https://na.op.gg/champion/{0}",
                                formattedChamp);
                                break;
                            // lolalytics
                            case 2:
                                // Need to replace the formatted champ name first, if needed
                                string fc = formattedChamp.ToLower();
                                if (this.m_lolalyticsChampNames.ContainsKey(fc))
                                    formattedChamp = this.m_lolalyticsChampNames[fc];

                                url1 = String.Format("https://lolalytics.com/ranked/worldwide/platinum/plus/champion/{0}",
                                formattedChamp);
                                break;
                            // champion.gg
                            case 3:
                                url1 = String.Format("https://champion.gg/champion/{0}",
                                formattedChamp);
                                break;
                        }

                        System.Diagnostics.Process.Start(url1);


                        // Check if the recently clicked on champion is already in the most
                        // recent champions selected
                        bool notInRecentList = true;

                        for (int j = 0, j2 = this.m_arrRecentlyPlayedChamps.Length; j < j2; j++)
                        {
                            String currentlySelected = this.currentlySelectedLvi.Content.ToString();
                            String curRecentChamp = this.m_arrRecentlyPlayedChamps[j].Content.ToString();

                            // If there is equal, switch
                            if (currentlySelected.Equals(curRecentChamp))
                            {
                                // Switch to top
                                CustomListViewItem temp = this.m_arrRecentlyPlayedChamps[j];
                                this.m_arrRecentlyPlayedChamps[j] = this.m_arrRecentlyPlayedChamps[0];
                                this.m_arrRecentlyPlayedChamps[0] = temp;
                                notInRecentList = false;

                                // Then refresh the list
                                this.refreshRecentChampList();
                                break;
                            }
                        }

                        // Store the champion in the most recent champions selected only if it is
                        // not in the list yet
                        if (notInRecentList)
                            this.storeRecentChampion(this.currentlySelectedLvi);
                    }
                }
            }
        }


        /// <summary>
        /// Store the champion selected into the five most recent champion list.
        /// </summary>
        /// <param name="listViewItemInput">The CustomListViewItem to add</param>
        private void storeRecentChampion(CustomListViewItem listViewItemInput)
        {
            // Add in the new item and shifting everything else to the right
            this.AddAndShift(listViewItemInput);

            this.refreshRecentChampList();
        }


        /// <summary>
        /// Clear the this.mostRecentChamps list, then re-add in the current list.
        /// </summary>
        private void refreshRecentChampList()
        {
            // Re-Add to the Most Recent Champs List
            this.mostRecentChamps.Items.Clear();
            foreach (CustomListViewItem item in this.m_arrRecentlyPlayedChamps)
            {
                // We need to add a new item as the first CustomListViewItem already has a parent
                CustomListViewItem item2 = new CustomListViewItem();
                item2.Content = item.Content;

                bool contentNull = String.IsNullOrEmpty(item.Content.ToString());

                // Only add a select handler if the content is not null
                if (false == contentNull)
                    item2.Selected += this.SelectItemHandler;

                // Set focusable to false if the content is null
                item2.Focusable = !contentNull;

                this.mostRecentChamps.Items.Add(item2);
            }
        }


        /// <summary>
        /// Add to the beginning of m_arrRecentlyPlayedChamps, then shift every item to the right
        /// </summary>
        /// <param name="setInput"></param>
        private void AddAndShift(CustomListViewItem listViewItemToAdd)
        {
            // Shift first
            for (int i = this.m_arrRecentlyPlayedChamps.Length - 1; i >= 1; i--)
            {
                this.m_arrRecentlyPlayedChamps[i] = this.m_arrRecentlyPlayedChamps[i - 1];
            }
            // Add to the beginning
            this.m_arrRecentlyPlayedChamps[0] = listViewItemToAdd;
        }


        /// <summary>
        /// Lowercase the character after a space or '
        /// </summary>
        /// <param name="formattedChamp"></param>
        /// <returns></returns>
        private String lowercaseChars(String formattedChamp)
        {
            // If there is a space or ', then lowercase the character right after
            // the space or '
            String[] delimiters = { " ", "'" };
            String currentlySelected = this.currentlySelectedLvi.Content.ToString();

            foreach (String delim in delimiters)
            {
                int index1 = formattedChamp.IndexOf(delim);
                if (index1 >= 0)
                {
                    int afterWhitespace = index1 + 1;
                    String s = currentlySelected[afterWhitespace].ToString().ToLower();

                    formattedChamp = currentlySelected.Substring(0, afterWhitespace)
                        + s + currentlySelected.Substring(afterWhitespace + 1);
                }
            }

            return formattedChamp;
        }

        /// <summary>
        /// Handle when the Window closes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Save settings to JSON file
            // Get the most recent champions
            int length1 = mostRecentChamps.Items.Count;
            List<String> recentChamps = new List<String>();

            for (int i = 0; i < length1; i++)
            {
                String content = ((CustomListViewItem)mostRecentChamps.Items[i]).Content.ToString();
                if (false == String.IsNullOrEmpty(content))
                {
                    recentChamps.Add(content);
                }
            }


            // Then get the sites selected
            length1 = this.m_siteNames.Length;
            bool[] tempSitesSelected = new bool[length1];
            for (int i = 0; i < length1; i++)
            {
                CheckBox checkBox = (CheckBox)this.FindName(this.m_siteNames[i]);
                tempSitesSelected[i] = (bool)checkBox.IsChecked;
            }

            SettingsJsonObject sjo = new SettingsJsonObject();
            sjo.fiveRecentChampions = recentChamps.ToArray();
            sjo.sitesSelected = tempSitesSelected;

            // Write to file
            // serialize JSON directly to a file
            using (System.IO.StreamWriter file = System.IO.File.CreateText(this.jsonSettingsFile))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, sjo);
            }
        }
    }
}
