using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace Text_Merger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 




    public partial class MainWindow : Window
    {

        static public ObservableCollection<Data_Row> Rows;

        static public Thread Work_Thread = new Thread(String_Combination.Work_Loop);

        public MainWindow()
        {
            InitializeComponent();

            Rows = new ObservableCollection<Data_Row>();

            if (Utilities.DoesFolderExist("Data"))
            {
                if (Utilities.DoesFileExist("Saved_Paths.txt", "Data"))
                {
                    //Load Config
                    List<string> temp_list_Path = new List<string>();
                    List<string> temp_list_Extra = new List<string>();

                    Utilities.MakeFolder("Data");
                    string[] path_array = Utilities.LoadLinesFromFile("Saved_Paths.txt", "Data");
                    string[] extra_array = Utilities.LoadLinesFromFile("Saved_Extra.txt", "Data");

                    int i = 0;
                    foreach (string path in path_array)
                    {
                        Data_Row new_row = new Data_Row();
                        new_row.Path_String = path_array[i];
                        new_row.Extra_Text = extra_array[i];
                        new_row.Index = i;

                        Rows.Add(new_row);

                        i += 1;
                    }

                }
            }
            else
            {
                for (int i = 0; i < 1; i++)
                {
                    Data_Row newRow = new Data_Row();
                    newRow.Index = Rows.Count;
                    newRow.Path_String = string.Empty;
                    newRow.Extra_Text = string.Empty;

                    Rows.Add(newRow);
                }
            }
            

            lvDataBinding.ItemsSource = Rows;

            Work_Thread.IsBackground = true;
            Work_Thread.Start();

        }

        static public void DeleteRow(int Index)
        {
            Console.WriteLine("removing row " + Index.ToString());
            Rows.RemoveAt(Index);

            int i = 0;
            foreach (Data_Row row in Rows)
            {
                row.Index = i;
                i += 1;
            }

        }


        private void button_newInput_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("new row");
            Data_Row newRow = new Data_Row();
            newRow.Index = Rows.Count;
            newRow.Path_String = string.Empty;
            newRow.Extra_Text = string.Empty;

            Rows.Add(newRow);

            lvDataBinding.ItemsSource = Rows;
        }


        private void DeleteRow_Command_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void DeleteRow_Command_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Console.WriteLine("The New command was invoked");


            Data_Row temp_row = (Data_Row)e.Parameter;
            int Index = temp_row.Index;
            Console.WriteLine(Index);
            DeleteRow(Index);
        }


        private void button_copyOutputPath_Click(object sender, RoutedEventArgs e)
        {

            if (!Utilities.DoesFolderExist("Merged_Text"))
            {
                Utilities.MakeFolder("Merged_Text");
                if (!Utilities.DoesFileExist("Merged_Text.txt", "Merged_Text"))
                {
                    Utilities.MakeFile("Merged_Text.txt", "Merged_Text");

                }
            }


            Utilities.OverWriteToFile("Merged_Text.txt", "Merged_Text", String_Combination.Merged_Text);


        }

    }

    public class Data_Row
    {
        public int Index { get; set; }
        public string Path_String { get; set; }
        public string Path_String_Text { get; set; }
        public string Extra_Text { get; set; }

        public void Load_Text_Files()
        {
            Console.WriteLine("Load_Text_Files for datarow: " + Path_String);
            if (!String.IsNullOrEmpty(Path_String))
            {

                try
                {
                    string filePath = Path_String;

                    FileStream filePathStream = new FileStream(filePath,
                    FileMode.Open,
                    FileAccess.Read);
                    int sizeP = (int)filePathStream.Length;
                    byte[] fileBytes = new byte[sizeP];
                    sizeP = filePathStream.Read(fileBytes, 0, sizeP);
                    filePathStream.Close();

                    string FileString = Encoding.UTF8.GetString(fileBytes);

                    Path_String_Text = FileString;
                }
                catch (Exception e)
                {
                    /*
                    Console.WriteLine("===========================================\r\nERROR\r\n===========================================");
                    Console.WriteLine(e);
                    Console.WriteLine("\r\n===========================================\r\n");
                    Console.WriteLine("Maybe missing files?");
                    Console.WriteLine("\r\n===========================================\r\n");
                    */

                }

            }


        }
    }

    static public class String_Combination
    {

        static public bool wook_bool = true;

        static public ObservableCollection<Data_Row> Rows_Copy;

        static public string Merged_Text = string.Empty;

        static public void Work_Loop()
        {


            while (wook_bool)
            {
                if (MainWindow.Rows.Count != 0)
                {
                    Rows_Copy = MainWindow.Rows;

                    Thread_Work();

                }

                Thread.Sleep(250);

            }

        }

        static public void Thread_Work()
        {
            if (!Utilities.DoesFolderExist("Data"))
            {
                Utilities.MakeFolder("Data");

            }
            Utilities.MakeFile("Saved_Paths.txt", "Data");
            Utilities.MakeFile("Saved_Extra.txt", "Data");

            if (!Utilities.DoesFolderExist("Merged_Text"))
            {
                Utilities.MakeFolder("Merged_Text");

            }
            Utilities.MakeFile("Merged_Text.txt", "Merged_Text");


            List<string> temp_list = new List<string>();
            foreach (Data_Row row in Rows_Copy)
            {
                row.Path_String_Text = string.Empty;
                if (!String.IsNullOrEmpty(row.Path_String))
                {
                    row.Load_Text_Files();
                }
                temp_list.Add(string.Concat(row.Path_String_Text, row.Extra_Text));

            }


            //Text Merger
            string temp_string = string.Empty;
            foreach (string text in temp_list)
            {
                temp_string = string.Concat(temp_string, text);
                Console.WriteLine(text);
            }
            Merged_Text = temp_string;

            Utilities.OverWriteToFile("Merged_Text.txt", "Merged_Text", Merged_Text);
            Console.WriteLine("Wrote to Merged File");

            List<string> temp_list_Path = new List<string>();
            List<string> temp_list_Extra = new List<string>();

            int counter_i = 0;
            int temp_i = Rows_Copy.Count;
            string[] temp_Path_array = new string[temp_i];
            string[] temp_Extra_array = new string[temp_i];
            foreach (Data_Row row in Rows_Copy)
            {

                temp_Path_array[counter_i] = row.Path_String;
                temp_Extra_array[counter_i] = row.Extra_Text;

                counter_i += 1;

            }

            



            Utilities.OverWriteToFile("Saved_Paths.txt", "Data", temp_Path_array);
            Utilities.OverWriteToFile("Saved_Extra.txt", "Data", temp_Extra_array);
        }




    }



    static public class Utilities
    {



        #region File Functions

        #region Load File
        static public byte[] LoadBytesFromFile(string FileName)
        {
            byte[] fileBytes;

            try
            {
                string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @FileName);

                FileStream filePathStream = new FileStream(filePath,
                FileMode.Open,
                FileAccess.Read);
                int sizeP = (int)filePathStream.Length;

                fileBytes = new byte[sizeP];
                sizeP = filePathStream.Read(fileBytes, 0, sizeP);

                filePathStream.Close();

                return fileBytes;
            }
            catch (Exception e)
            {
                Console.WriteLine("===========================================\r\nERROR\r\n===========================================");
                Console.WriteLine(e);
                Console.WriteLine("\r\n===========================================\r\n");
                Console.WriteLine("Maybe missing files?");
                Console.WriteLine("\r\n===========================================\r\n");
                fileBytes = new byte[0];

                return fileBytes;
            }

        }
        static public byte[] LoadBytesFromFile(string FileName, string FilePath)
        {
            byte[] fileBytes;

            try
            {
                string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FilePath, @FileName);

                FileStream filePathStream = new FileStream(filePath,
                FileMode.Open,
                FileAccess.Read);
                int sizeP = (int)filePathStream.Length;

                fileBytes = new byte[sizeP];
                sizeP = filePathStream.Read(fileBytes, 0, sizeP);

                filePathStream.Close();

                return fileBytes;
            }
            catch (Exception e)
            {
                Console.WriteLine("===========================================\r\nERROR\r\n===========================================");
                Console.WriteLine(e);
                Console.WriteLine("\r\n===========================================\r\n");
                Console.WriteLine("Maybe missing files?");
                Console.WriteLine("\r\n===========================================\r\n");
                fileBytes = new byte[0];

                return fileBytes;
            }

        }



        static public string LoadFromFile(string FileName)
        {
            try
            {
                string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @FileName);

                FileStream filePathStream = new FileStream(filePath,
                FileMode.Open,
                FileAccess.Read);
                int sizeP = (int)filePathStream.Length;
                byte[] fileBytes = new byte[sizeP];
                sizeP = filePathStream.Read(fileBytes, 0, sizeP);
                filePathStream.Close();

                string FileString = Encoding.UTF8.GetString(fileBytes);

                return FileString;
            }
            catch (Exception e)
            {
                Console.WriteLine("===========================================\r\nERROR\r\n===========================================");
                Console.WriteLine(e);
                Console.WriteLine("\r\n===========================================\r\n");
                Console.WriteLine("Maybe missing files?");
                Console.WriteLine("\r\n===========================================\r\n");

                return string.Empty;
            }

        }
        static public string LoadFromFile(string FileName, string FilePath)
        {
            try
            {
                string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FilePath, @FileName);

                FileStream filePathStream = new FileStream(filePath,
                FileMode.Open,
                FileAccess.Read);
                int sizeP = (int)filePathStream.Length;
                byte[] fileBytes = new byte[sizeP];
                sizeP = filePathStream.Read(fileBytes, 0, sizeP);
                filePathStream.Close();

                string FileString = Encoding.UTF8.GetString(fileBytes);

                return FileString;
            }
            catch (Exception e)
            {
                Console.WriteLine("===========================================\r\nERROR\r\n===========================================");
                Console.WriteLine(e);
                Console.WriteLine("\r\n===========================================\r\n");
                Console.WriteLine("Maybe missing files?");
                Console.WriteLine("\r\n===========================================\r\n");

                return string.Empty;
            }

        }


        static public string[] LoadLinesFromFile(string FileName)
        {
            string[] fileResults = new string[0];

            try
            {
                string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @FileName);

                fileResults = File.ReadAllLines(filePath);


                return fileResults;
            }
            catch (Exception e)
            {
                Console.WriteLine("===========================================\r\nERROR\r\n===========================================");
                Console.WriteLine(e);
                Console.WriteLine("\r\n===========================================\r\n");
                Console.WriteLine("Maybe missing files?");
                Console.WriteLine("\r\n===========================================\r\n");

                return fileResults;
            }

        }
        static public string[] LoadLinesFromFile(string FileName, string FilePath)
        {
            string[] fileResults = new string[0];

            try
            {
                string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FilePath, @FileName);

                fileResults = File.ReadAllLines(filePath);


                return fileResults;
            }
            catch (Exception e)
            {
                Console.WriteLine("===========================================\r\nERROR\r\n===========================================");
                Console.WriteLine(e);
                Console.WriteLine("\r\n===========================================\r\n");
                Console.WriteLine("Maybe missing files?");
                Console.WriteLine("\r\n===========================================\r\n");

                return fileResults;
            }

        }
        #endregion

        #region Make File
        static public void MakeFile(string FileName, string FilePath)
        {
            if (!DoesFolderExist(FilePath))
            {
                MakeFolder(FilePath);
            }
            MakeFile(FileName, FilePath, String.Empty);

        }
        static public void MakeFile(string FileName, string FilePath, byte[] FileData)
        {
            if (!DoesFolderExist(FilePath))
            {
                MakeFolder(FilePath);
            }
            string newFilePath = System.IO.Path.Combine(FilePath, FileName);

            if (!File.Exists(newFilePath))
            {

                // Create a file to write to.
                using (FileStream sw = File.Create(AppDomain.CurrentDomain.BaseDirectory + newFilePath))
                {
                    sw.Write(FileData, 0, FileData.Length);

                }
            }


        }
        static public void MakeFile(string FileName, string FilePath, string FileData)
        {
            if (!DoesFolderExist(FilePath))
            {
                MakeFolder(FilePath);
            }
            string newFilePath = System.IO.Path.Combine(FilePath, FileName);

            if (!File.Exists(newFilePath))
            {
                using (StreamWriter sw = File.CreateText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory + newFilePath)))
                {
                    sw.WriteLine(FileData);
                }
            }
        }
        static public void MakeFile(string FileName, string FilePath, string[] FileData)
        {
            if (!DoesFolderExist(FilePath))
            {
                MakeFolder(FilePath);
            }
            string newFilePath = System.IO.Path.Combine(FilePath, FileName);
            if (!File.Exists(newFilePath))
            {
                using (StreamWriter sw = File.CreateText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, newFilePath)))
                {
                    foreach (string line in FileData)
                    {
                        sw.WriteLine(line);
                    }
                }
            }
        }
        #endregion

        #region Write to File
        static public void WriteToFile(string FileName, string FilePath, string FileData)
        {
            string newFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FilePath, FileName);

            if (File.Exists(newFilePath))
            {
                using (StreamWriter sw = File.AppendText(newFilePath))
                {
                    sw.WriteLine(FileData);

                }
            }


        }
        static public void WriteToFile(string FileName, string FilePath, string[] FileData)
        {
            string newFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FilePath, FileName);

            if (File.Exists(newFilePath))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.AppendText(newFilePath))
                {
                    foreach (string line in FileData)
                    {
                        sw.WriteLine(line);

                    }

                }
            }


        }
        #endregion

        #region OverWrite the File
        static public void OverWriteToFile(string FileName, string FilePath, byte[] FileData)
        {
            string newFilePath = System.IO.Path.Combine(FilePath, FileName);

            if (File.Exists(newFilePath))
            {

                // Create a file to write to.
                using (FileStream sw = File.Create(AppDomain.CurrentDomain.BaseDirectory + newFilePath))
                {
                    sw.Write(FileData, 0, FileData.Length);

                }
            }


        }

        static public void OverWriteToFile(string FileName, string FilePath, string FileData)
        {
            string newFilePath = System.IO.Path.Combine(FilePath, FileName);

            if (File.Exists(newFilePath))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(AppDomain.CurrentDomain.BaseDirectory + newFilePath))
                {
                    sw.WriteLine(FileData);

                }
            }


        }
        static public void OverWriteToFile(string FileName, string FilePath, string[] FileData)
        {
            string newFilePath = System.IO.Path.Combine(FilePath, FileName);

            if (File.Exists(newFilePath))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(AppDomain.CurrentDomain.BaseDirectory + newFilePath))
                {
                    foreach (string line in FileData)
                    {
                        sw.WriteLine(line);

                    }

                }
            }


        }
        #endregion


        #region Does File Exist
        static public bool DoesFileExist(string FileName, string FilePath)
        {
            bool doesFileExist = false;
            string newFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FilePath, FileName);

            if (File.Exists(newFilePath))
            {
                doesFileExist = true;
            }

            return doesFileExist;
        }
        #endregion

        #region Delete File

        static public void DeleteFile(string FileName, string FilePath)
        {
            string newFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FilePath, FileName);
            if (File.Exists(newFilePath))
            {

                File.Delete(newFilePath);

            }
        }

        #endregion

        #region Make Folder

        public static void MakeFolder(string folderName)
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderName);
            System.IO.Directory.CreateDirectory(path);

        }
        #endregion

        #region Does Folder Exist
        static public bool DoesFolderExist(string folderPath)
        {
            bool doesFolderExist = false;

            if (Directory.Exists(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderPath)))
            {
                doesFolderExist = true;
            }

            return doesFolderExist;
        }
        #endregion

        #region Delete Folder

        static public void DeleteFolder(string FileName, string FilePath)
        {
            string newFilePath = System.IO.Path.Combine(FilePath, FileName);
            if (Directory.Exists(newFilePath))
            {

                Directory.Delete(newFilePath);

            }
        }

        #endregion



        #endregion




    }




}
