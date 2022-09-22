using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace Spider
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string TMP = "LangUtils.Get({0})";

        //private const string REG = "(\"|')(.*?)[\u4e00-\u9fa5]+(.*?)\\1";
        private const string REG = "(\"|')([^\n\"']*?)[\u4e00-\u9fa5]+(.*?)\\1";
        public List<MatchTextVO> matchTextList = new List<MatchTextVO>();
        private int position = -1;

        private List<string> elseFileList = new List<string>()
        {
            "cliproto.bytes",
            "/Editor/"
        };

        private List<string> elseBeginWith = new List<string>()
        {
            "///","//",
            "---","--"
        };

        public List<string> searchPars = new List<string>
        {
            "*.bytes","*.cs","*.lua"
        };

        public MainWindow()
        {
            InitializeComponent();

            this.replaceAndNextBtn.Visibility =this.nextBtn.Visibility= this.prevBtn.Visibility = Visibility.Collapsed;

            CompositionTarget.Rendering += Update;
        }

        private void Update(object sender, EventArgs e)
        {
            if (isAutoCb.IsChecked==true)
            {
                if (isOnlySaveLangCb.IsChecked == true)
                {
                    onlySaveLangBtn_Click(null, null);
                }
                else
                {
                    replaceAndNextBtn_Click(null, null);
                }
            }
        }

        private MatchTextVO _currentMatchText;
        private MatchFileVO currentMatchFile;

        private void doNext()
        {
            if (position >= matchTextList.Count)
            {
                this.contentTF.Text = "";
                this.replaceAndNextBtn.Visibility=this.nextBtn.Visibility = Visibility.Collapsed;
                return;
            }

            position++;
            checkButton();
            doSelectedText();
        }

        private void doSelectedText()
        {
            if (position >= matchTextList.Count || position < 0)
            {
                if (currentMatchFile != null)
                {
                    currentMatchFile.Save();
                    _currentMatchText = null;
                }
                return;
            }

            _currentMatchText = matchTextList[position];

            var file = _currentMatchText.file;
            if (file != currentMatchFile)
            {
                if (currentMatchFile != null)
                {
                    currentMatchFile.Save();
                }
            }
            currentMatchFile = file;

            this.filePathTF.Text = file.filePath;
            this.contentTF.Text = file.Load();

            var line = _currentMatchText.line;

            var oldContent = currentMatchFile.lines[line].rawContent;
            var index = oldContent.IndexOf(_currentMatchText.value, _currentMatchText.index);
            var lineIndex = this.contentTF.GetCharacterIndexFromLineIndex(line);
            this.contentTF.Select(lineIndex + index, _currentMatchText.value.Length);

            this.contentTF.ScrollToLine(line);
            this.contentTF.Focus();
        }

        private void checkButton()
        {
            this.prevBtn.Visibility = (position > 0) ? Visibility.Visible : Visibility.Collapsed;
            this.replaceAndNextBtn.Visibility =this.nextBtn.Visibility= (position >= 0 && position < matchTextList.Count)
                ? Visibility.Visible
                : Visibility.Collapsed;

            this.processBar.Value = (position / (matchTextList.Count * 1.0f)) * 100;
        }

        private MatchFileVO MatchFile(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            var len = lines.Length;
            if (len==0)
            {
                return null;
            }

            var matchFile = new MatchFileVO(len)
            {
                filePath = filePath,
            };

            var extension=Path.GetExtension(filePath);

            ///lua文件为配置文件
            if (extension == ".lua")
            {
                matchFile.skipReplace = true;
            }

            var has = false;
            var reg = new Regex(REG, RegexOptions.IgnoreCase);
            for (int i = 0; i < len; i++)
            {
                var lineValue = lines[i];
                var line = new MatchLineVO()
                {
                    rawContent = lineValue
                };
                matchFile.lines[i] = line;

                var trim = lineValue.Trim();
                if (trim == "")
                {
                    continue;
                }
                bool beginWith = false;
                foreach (var begin in elseBeginWith)
                {
                    if (trim.IndexOf(begin) == 0)
                    {
                        beginWith=true;
                        break;
                    }
                }

                if (beginWith || trim.IndexOf("Log.") !=-1|| trim.IndexOf("Debug.") != -1)
                {
                    continue;
                }
                if(extension==".cs" && trim.IndexOf("[") == 0)
                {
                    continue;
                }

                var match = reg.Match(lineValue, 0);
                while (match.Success)
                {
                    var index = match.Index;
                    var value = match.Value;
                    var matchText = new MatchTextVO()
                    {
                        line=i,
                        file = matchFile,
                        index = index,
                        value = value
                    };

                    has = true;
                    line.matchTexts.Add(matchText);

                    match = reg.Match(lineValue, index + value.Length);
                }
            }

            if (has == false)
            {
                return null;
            }
            return matchFile;
        }

        private void replaceAndNextBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentMatchText == null)
            {
                return;
            }
            var file = _currentMatchText.file;

            if (file.skipReplace==false)
            {

                var TMPLen = TMP.Length - 3;
                var oldValue = _currentMatchText.value;
                var newValue = string.Format(TMP, oldValue);

                var oldContent = file.lines[_currentMatchText.line].rawContent;

                var newIndex = _currentMatchText.index - TMPLen + 1;
                var isReplaced = oldContent.IndexOf(newValue, newIndex); //后扩号不要偏
                if (isReplaced == -1 || isReplaced > _currentMatchText.index)
                {
                    var newContent =
                        oldContent.ReplaceAt(_currentMatchText.index, _currentMatchText.value.Length, newValue);
                    file.lines[_currentMatchText.line].rawContent = newContent;
                    var line = file.lines[_currentMatchText.line];
                    var has = false;
                    foreach (var matchText in line.matchTexts)
                    {
                        if (matchText == _currentMatchText)
                        {
                            has = true;
                            matchText.index += TMPLen - 1;
                            continue;
                        }

                        if (has)
                        {
                            matchText.index += TMPLen;
                        }
                    }

                    this.contentTF.Text = file.Load();
                }
            }

            doNext();
        }

        private void prevBtn_Click(object sender, RoutedEventArgs e)
        {
            if (position <= 0)
            {
                this.prevBtn.Visibility = Visibility.Collapsed;
                return;
            }

            position--;
            checkButton();
            doSelectedText();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            DoFiles(files);
        }

        private void DoFiles(string[] files)
        {
            position = -1;
            matchTextList.Clear();

            foreach (var file in files)
            {
                var filePath = file.Replace("\\", "/");
                if (Directory.Exists(filePath))
                {
                    var list = new List<string>();
                    foreach (var item in searchPars)
                    {
                        var tempFiles = Directory.GetFiles(filePath, item, SearchOption.AllDirectories);
                        if (tempFiles.Length > 0)
                        {
                            list.AddRange(tempFiles);
                        }
                    }

                    foreach (var item in list)
                    {
                        filePath = item.Replace("\\", "/");
                        if (elseFileListContainer(filePath))
                        {
                            continue;
                        }
                        var matchFile = MatchFile(filePath);
                        if (matchFile != null)
                        {
                            matchTextList.AddRange(matchFile.getMatchTexts());
                        }
                    }
                }
                else
                {
                    if (elseFileListContainer(filePath))
                    {
                        continue;
                    }
                    var matchFile = MatchFile(filePath);
                    if (matchFile != null)
                    {
                        matchTextList.AddRange(matchFile.getMatchTexts());
                    }
                }
            }

            if (matchTextList.Count > 0)
            {
                doNext();
            }
            else
            {
                MessageBox.Show("没有匹配的中文字符");
            }
        }

        private bool elseFileListContainer(string fileName)
        {
            foreach (var elseFileName in elseFileList)
            {
                var b = fileName.IndexOf(elseFileName) != -1;
                if (b)
                {
                    return b;
                }
            }
            return false;
        }

        private void nextBtn_Click(object sender, RoutedEventArgs e)
        {
            doNext();
        }

        private void onlySaveLangBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentMatchFile != null)
            {
                currentMatchFile.Save();
                while (position < matchTextList.Count-1)
                {
                    var matchText = matchTextList[position++];
                    if (matchText.file != currentMatchFile)
                    {
                        position--;
                        break;
                    }
                }
                doNext();
            }
        }
    }
}
