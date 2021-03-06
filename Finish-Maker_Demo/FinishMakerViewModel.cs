﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Threading;
using System.Diagnostics;
using Microsoft.Win32;
using System.Configuration;

namespace Finish_Maker_Demo
{
    class FinishMakerViewModel : INotifyPropertyChanged
    {

        FinishMakerModel finishMaker = new FinishMakerModel();
        Configuration config;
        public ObservableCollection<ExportLinks> ExpLinksList { get; set; }
        public ObservableCollection<PD> PDList { get; set; }
        public ObservableCollection<ID> IDList { get; set; }
        public ObservableCollection<ChildTitleDuplicates> ChtDuplicatesList { get; set; }

        Dispatcher dispatcher;
        public FinishMakerViewModel()
        {
            var configFileMap = new ExeConfigurationFileMap();
            configFileMap.ExeConfigFilename = "FinishMaker.exe.config";
            config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            ExpLinksList = new ObservableCollection<ExportLinks>();
            PDList = new ObservableCollection<PD>();
            IDList = new ObservableCollection<ID>();
            ChtDuplicatesList = new ObservableCollection<ChildTitleDuplicates>();
            finishMaker.ProductDataCheck = true;
            finishMaker.ValidateFiles = true;
            StartButton = "Run";
            UserName = GetUserName(config);
            finishMaker.Version = GetVersion();
            dispatcher = Dispatcher.CurrentDispatcher;

            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
        }
        private string GetVersion()
        {
            string configVersion = config.AppSettings.Settings["version"].Value;
            return configVersion;
        }
        private string GetUserName(Configuration config)
        {
            string userName = config.AppSettings.Settings["name"].Value;
            return userName;
        }
        private void SetUserName(Configuration config)
        {
            config.AppSettings.Settings["name"].Value = UserName;
            config.Save(ConfigurationSaveMode.Modified);
        }

        private RelayCommand addExpLinksCommand;
        private RelayCommand addPDCommand;
        private RelayCommand addIDCommand;
        private RelayCommand addChtDuplicatesCommand;
        private RelayCommand start;
        private RelayCommand deleteCommand;
        private ConsoleText consoleTextProperty;
        public RelayCommand AddExpLinksCommand
        {
            get
            {
                return addExpLinksCommand ??
                  (addExpLinksCommand = new RelayCommand(obj =>
                  {
                      OpenFileDialog openFileDialog = new OpenFileDialog { Multiselect = true };
                      if (openFileDialog.ShowDialog() == true)
                      {
                          foreach (string file in openFileDialog.FileNames)
                          {
                              ExportLinks exportLinks = new ExportLinks();
                              exportLinks.Path = file;
                              exportLinks.ViewPath = file.Substring(file.LastIndexOf("\\") + 1);
                              exportLinks.ID = ExpLinksList.Count + 1;
                              ExpLinksList.Add(exportLinks);
                          }
                      }
                  }));
            }
        }
        public RelayCommand AddPDCommand
        {
            get
            {
                return addPDCommand ??
                  (addPDCommand = new RelayCommand(obj =>
                  {
                      OpenFileDialog openFileDialog = new OpenFileDialog { Multiselect = true };
                      if (openFileDialog.ShowDialog() == true)
                      {
                          foreach (string file in openFileDialog.FileNames)
                          {
                              PD pdLinks = new PD();
                              pdLinks.Path = file;
                              pdLinks.ViewPath = file.Substring(file.LastIndexOf("\\") + 1);
                              pdLinks.ID = PDList.Count + 1;
                              PDList.Add(pdLinks);
                          }
                      }
                  }));
            }
        }
        public RelayCommand AddIDCommand
        {
            get
            {
                return addIDCommand ??
                  (addIDCommand = new RelayCommand(obj =>
                  {
                      OpenFileDialog openFileDialog = new OpenFileDialog { Multiselect = true };
                      if (openFileDialog.ShowDialog() == true)
                      {
                          foreach (string file in openFileDialog.FileNames)
                          {
                              ID idList = new ID();
                              idList.Path = file;
                              idList.ViewPath = file.Substring(file.LastIndexOf("\\") + 1);
                              idList.ID = IDList.Count + 1;
                              IDList.Add(idList);
                          }
                      }
                  }));
            }
        }
        public RelayCommand AddChtDuplicatesCommand
        {
            get
            {
                return addChtDuplicatesCommand ??
                  (addChtDuplicatesCommand = new RelayCommand(obj =>
                  {
                      OpenFileDialog openFileDialog = new OpenFileDialog { Multiselect = true };
                      if (openFileDialog.ShowDialog() == true)
                      {
                          foreach (string file in openFileDialog.FileNames)
                          {
                              ChildTitleDuplicates chtList = new ChildTitleDuplicates();
                              chtList.Path = file;
                              chtList.ViewPath = file.Substring(file.LastIndexOf("\\") + 1);
                              chtList.ID = ChtDuplicatesList.Count + 1;
                              ChtDuplicatesList.Add(chtList);
                          }
                      }
                  }));
            }
        }
        public bool IsSelectedExpLinkCheck
        {
            get { return finishMaker.ExportLinkCheck; }
            set
            {
                finishMaker.ExportLinkCheck = value;

                if (finishMaker.ProductDataCheck == value)
                {
                    IsSelectedPDCheck = !value;
                }

                OnPropertyChanged("IsSelectedExpLinkCheck");
            }
        }
        public bool IsSelectedPDCheck
        {
            get { return finishMaker.ProductDataCheck; }
            set
            {
                finishMaker.ProductDataCheck = value;

                if (finishMaker.ExportLinkCheck == value)
                {
                    IsSelectedExpLinkCheck = !value;
                }
                
                OnPropertyChanged("IsSelectedPDCheck");
            }
        }
        public bool ValidateFiles
        {
            get { return finishMaker.ValidateFiles; }
            set
            {
                finishMaker.ValidateFiles = value;
                OnPropertyChanged("ValidateFiles");
            }
        }
        public bool FitmentUpdateCheck
        {
            get { return finishMaker.FitmentUpdate; }
            set
            {
                finishMaker.FitmentUpdate = value;
                OnPropertyChanged("FitmentUpdateCheck");
            }
        }
        public string StartButton
        {
            get { return finishMaker.StartButton; }
            set
            {
                finishMaker.StartButton = value;
                OnPropertyChanged("StartButton");
            }
        }
        public RelayCommand Start
        {
            get
            {
                return start ??
                  (start = new RelayCommand(obj =>
                  {
                      worker.RunWorkerAsync();
                  }));
            }
        }
        public RelayCommand DeleteCommand
        {
            get
            {
                return deleteCommand ?? (deleteCommand = new RelayCommand(obj =>
                {
                    Files currentFile = obj as Files;
                    if (currentFile != null)
                    {

                        if (currentFile is ExportLinks)
                        {
                            ExpLinksList.Remove(currentFile as ExportLinks);
                        }
                        else if (currentFile is PD)
                        {
                            PDList.Remove(currentFile as PD);
                        }
                        else if (currentFile is ID)
                        {
                            IDList.Remove(currentFile as ID);
                        }
                        else if (currentFile is ChildTitleDuplicates)
                        {
                            ChtDuplicatesList.Remove(currentFile as ChildTitleDuplicates);
                        }

                    }
                }));
            }
        }
        public string UserName
        {
            get { return finishMaker.UserName; }
            set
            {
                finishMaker.UserName = value;
                OnPropertyChanged("UserName");
            }
        }
        public string Version
        {
            get { return finishMaker.Version; }
            set
            {
                finishMaker.Version = value;
                OnPropertyChanged("Version");
            }
        }
        public ConsoleText ConsoleTextProperty
        {
            get { return consoleTextProperty; }
            set
            {
                consoleTextProperty = value;
                OnPropertyChanged("ConsoleTextProperty");
            }
        }
        public int Progress
        {
            get
            {
                return finishMaker.Progress;
            }
            set
            {
                finishMaker.Progress = value;
                OnPropertyChanged("Progress");
            }
        }

        private BackgroundWorker worker;
        private void changeProgress(int count)
        {
            this.worker.ReportProgress(count);
        }
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if(!checkIfNotFirstStart())
                return;
            OpenFileDialog fileDialog = GetFileDialog();

            if (fileDialog.ShowDialog() == true)
            {
                try
                {
                    ConsoleMessage message = new ConsoleMessage();
                    message.MessageNotification += MessageTriger;
                    string saveFilePath = Path.GetDirectoryName(fileDialog.FileName);
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    ConsoleTextProperty = new ConsoleText { TheText = "In progress..." + Environment.NewLine, TheColor = Brushes.Black };
                    List<List<string>> filePath = GetAllPathes(ExpLinksList, PDList, IDList, ChtDuplicatesList);

                    FileReader fileReader = new FileReader(filePath, IsSelectedPDCheck, message);
                    changeProgress(10);
                    Mistakes mistakesCheck = new Mistakes(fileReader, filePath, IsSelectedPDCheck);
                    if (CheckForCriticalErrors(mistakesCheck))
                        return;
                    Processing processing = new Processing(fileReader, UserName, FitmentUpdateCheck, message);
                    Writer writer = new Writer(processing, changeProgress, saveFilePath, message, fileReader);
                    writer.Write();

                    CheckAllErrors(mistakesCheck);
                    changeProgress(100);

                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
                    ConsoleText workingTime = ConsoleTextProperty;
                    workingTime.TheText += Environment.NewLine + "Время работы программы: " + elapsedTime;
                    ConsoleTextProperty = workingTime;
                    StartButton = "Clear";
                }
                catch (Exception ex)
                {
                    ConsoleTextProperty = new ConsoleText { TheText = "Произошла ошибка: " + ex, TheColor = Brushes.Red };
                }
            }
            SetUserName(config);
        }
        private void CheckAllErrors(Mistakes mistakesCheck)
        {
            if (ValidateFiles)
            {
                if (mistakesCheck.OtherErrors != null)
                    ConsoleTextProperty = new ConsoleText { TheColor = Brushes.Red, TheText = mistakesCheck.OtherErrors + "Done" };
                else
                    ConsoleTextProperty = new ConsoleText { TheColor = Brushes.Black, TheText = "Done" };
            }
            else
                ConsoleTextProperty = new ConsoleText { TheColor = Brushes.Black, TheText = "Done" };
        }
        private bool CheckForCriticalErrors(Mistakes mistakesCheck)
        {
            if (ValidateFiles)
            {
                if (mistakesCheck.CriticalErrors != null)
                {
                    ClearProperties(mistakesCheck.CriticalErrors);
                    return true;
                }
            }
            return false;
        }
        private OpenFileDialog GetFileDialog()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.ValidateNames = false;
            fileDialog.CheckFileExists = false;
            fileDialog.CheckPathExists = true;
            fileDialog.FileName = "Folder Selection.";
            return fileDialog;
        }
        private bool checkIfNotFirstStart()
        {
            if (StartButton == "Clear")
            {
                ClearProperties();
                return false;
            }
            else
                return true;
        }
        private void ClearProperties(string text="")
        {
            changeProgress(0);
            dispatcher.BeginInvoke(new Action(() =>
            {
                ExpLinksList.Clear();
                PDList.Clear();
                IDList.Clear();
                ChtDuplicatesList.Clear();
            }));
            StartButton = "Run";
            ConsoleTextProperty = new ConsoleText { TheColor = Brushes.Red, TheText = text };
        }
        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage;
        }
        private List<List<string>> GetAllPathes(ObservableCollection<ExportLinks> expLinkPath, ObservableCollection<PD> pdPath, ObservableCollection<ID> idPath, ObservableCollection<ChildTitleDuplicates> chtPath)
        {
            var expLinkNames = from exp in expLinkPath select exp.Path;
            var pdNames = from pd in pdPath select pd.Path;
            var idNames = from id in idPath select id.Path;
            var chtNames = from cht in chtPath select cht.Path;
            List<List<string>> result = new List<List<string>>();
            result.Add(expLinkNames.ToList());
            result.Add(pdNames.ToList());
            result.Add(idNames.ToList());
            result.Add(chtNames.ToList());
            return result;
        }
        private void MessageTriger(string message)
        {
            ConsoleTextProperty = new ConsoleText { TheColor = Brushes.Black, TheText = message };
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
