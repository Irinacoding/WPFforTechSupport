using System;
using System.Collections.Generic;
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
using System.Threading;


namespace PolicyService_v._1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void On_NameChanging(object sender, RoutedEventArgs e)
        {
           bool error = false;
            try
            {
                int count = ValidationImplementing();
                if (count > 0)
                {
                    throw new ArgumentException("Заполните отмеченное поле ввода.");
                }
            }
            catch (ArgumentException ex)
            {
                InfoBox.Clear();
                InfoBox.Text = ex.Message;
                error = true;
            }
            if (!error)
            {
                bool result = false;
                string NUMBER = number.Text;
                string oldName = nameBefore.Text;
                string newName = nameAfter.Text;
                string SerialBefore = serialBefore.Text;
                Guid policyID = VirtuDB.PolicyIDGetter(NUMBER, SerialBefore);
                result = VirtuDB.InsuredNameChanging(policyID, oldName, newName);
                if (result)
                {
                    InfoBox.Clear();
                    InfoBox.Text = "Изменение Фамилии, Имени прошло успешно.";
                }
                else
                {
                    InfoBox.Clear();
                    InfoBox.Text = "Попытка изменения Фамилии, Имени завершилась неудачей.";
                }
            }
        }

        private void On_BirthDateChanging(object sender, RoutedEventArgs e)
        {
            bool error = false;
            try
            {
                int count = ValidationImplementing();
                if (count > 0)
                {
                    throw new ArgumentException("Заполните отмеченное поле ввода.");
                }
            }
            catch (ArgumentException ex)
            {
                InfoBox.Clear();
                InfoBox.Text = ex.Message;
                error = true;
            }
            if (!error)
            {
                bool result = false;
                string NUMBER = number.Text;
                string SerialBefore = serialBefore.Text;
                string newBirthDate = birthDateAfter.Text;
                string oldBirthDate = birthDate.Text;
                string Name = name.Text;
                Guid policyID = VirtuDB.PolicyIDGetter(NUMBER, SerialBefore);
                result = VirtuDB.BirthDateChanging(policyID, newBirthDate, Name, oldBirthDate);
                if (result)
                {
                    InfoBox.Clear();
                    InfoBox.Text = "Изменение Даты Рождения прошло успешно.";
                }
                else
                {
                    InfoBox.Clear();
                    InfoBox.Text = "Попытка изменения Даты Рождения завершилась неудачей.";
                }
            }
        }

        private void On_SerialChanging(object sender, RoutedEventArgs e)
        {
            bool error = false;
            try
            {
                int count = ValidationImplementing();
                if (count > 0)
                {
                    throw new ArgumentException("Заполните отмеченное поле ввода.");
                }
            }
            catch (ArgumentException ex)
            {
                InfoBox.Clear();
                InfoBox.Text = ex.Message;
                error = true;
            }
            if (!error)
            {
                string NUMBER = number.Text;
                string SERIAL = serialAfetr.Text;
                string SerialBefore = serialBefore.Text;
                bool resultFromPolicyRegistry = false;
                bool resultFromPolicyBodyChanging = false;

                Guid policyID = VirtuDB.PolicyIDGetter(NUMBER, SerialBefore);

                bool resultFromDocumentDataDB = VirtuDB.SERIALInDocumentDataUpdating(policyID, SERIAL);
                if (resultFromDocumentDataDB)
                {
                    resultFromPolicyRegistry = VirtuDB.SERIALInPolicyRegistryUpdating(policyID, SERIAL);
                }

                if (resultFromPolicyRegistry)
                {
                    resultFromPolicyBodyChanging = VirtuDB.SERIALInPolicyBodyChanging(NUMBER, SERIAL, policyID);
                }

                if (resultFromPolicyRegistry && resultFromPolicyBodyChanging && resultFromDocumentDataDB)
                {
                    InfoBox.Clear();
                    InfoBox.Text = "Серия полиса была успешна изменена";
                }
                else
                {
                    InfoBox.Clear();
                    InfoBox.Text = "Попытка изменения серии завершилась неудачей.";
                }
            }
        }

        /// <summary>
        /// Event Handler For btn_"Получить название Родительского продукта"
        /// </summary>
        /// <param name="sender">btn5</param>
        /// <param name="e"></param>
        private void On_ParentIDReceiving(object sender, RoutedEventArgs e)
        {
            bool error = false;
            try
            {
                int count = ValidationImplementing();
                if (count > 0)
                {
                    throw new ArgumentException("Заполните отмеченное поле ввода.");
                }
            }
            catch (ArgumentException ex)
            {
                InfoBox.Clear();
                InfoBox.Text = ex.Message;
                error = true;
            }
            if (!error)
            {
                string Number = number.Text;
                Guid ProductID = VirtuDB.ProductIDGetter(Number);
                List<ObjectTreeItem> parentList = new List<ObjectTreeItem>();
                parentList = VirtuDB.ParentProductIDGetter(ProductID);
                if (ProductID != null)
                {
                    productID.Clear();
                    productID.Text = ProductID.ToString();
                }
                else
                {
                    InfoBox.Clear();
                    InfoBox.Text = "Неудалось извлечь значение ID продукта.";
                }
                if (parentList == null)
                {
                    InfoBox.Clear();
                    InfoBox.Text = "Операция завершилась неудачей";
                }
                else
                {
                    int count = parentList.Count<ObjectTreeItem>();
                    switch (count)
                    {
                        case 0:
                            parentProductName.Clear();
                            parentProductName.Text = "Операция завершилась неудачей";
                            break;
                        case 1:
                            parentProductName.Clear();
                            parentProductName.Text = "Родительский продукт:  " + parentList[0].Name;
                            break;
                        default:
                            parentProductName.Clear();
                            parentProductName.Text = "Родительский продукт:  " + parentList[1].Name;
                            break;
                    }
                    InfoBox.Clear();
                    InfoBox.AppendText("Иерархия продукта:\r\n");
                    foreach (ObjectTreeItem item in parentList)
                    {
                        InfoBox.AppendText(item.DisplayName + "\r\n");
                    }
                }
            }
        }


        private void On_VipVerification(object sender, RoutedEventArgs e)
        {
           bool error = false;
            try
            {
                int count = ValidationImplementing();
                if (count > 0)
                {
                    throw new ArgumentException("Заполните отмеченное поле ввода.");
                }
            }
            catch (ArgumentException ex)
            {
                InfoBox.Clear();
                InfoBox.Text = ex.Message;
                error = true;
            }
            if (!error)
            {
                string Name = name.Text;
                Guid vipID = default(Guid);

                if (string.IsNullOrEmpty(Name))
                {
                    InfoBox.Clear();
                    InfoBox.Text = "Основное поле Фамилия Имя Застрахованного не должно быть пустым";
                }
                else
                {
                    vipID = VirtuDB.VipIDGetter(Name);
                }
                if (vipID != null && vipID != default(Guid))
                {
                    InfoBox.Clear();
                    InfoBox.Text = "Застрахованный значится как ВИП";
                }
                else
                {
                    InfoBox.Clear();
                    InfoBox.Text = "Застрахованный не значится как ВИП";
                }
            }
        }


        private void On_VipDBAdd(object sender, RoutedEventArgs e)
        {
           bool error = false;
            try
            {
                int count = ValidationImplementing();
                if (count > 0)
                {
                    throw new ArgumentException("Заполните отмеченное поле ввода.");
                }
            }
            catch (ArgumentException ex)
            {
                InfoBox.Clear();
                InfoBox.Text = ex.Message;
                error = true;
            }
            if (!error)
            {
                bool result = false;
                string Name = name.Text;
                string BirthDate = birthDate.Text;

                result = VirtuDB.AddToRGS_Travel_Vip(Name, BirthDate);

                if (result)
                {
                    InfoBox.Clear();
                    InfoBox.Text = "Застрахованный был успешно добавлен в RGS_Travel_VIP";
                }
                else
                {
                    InfoBox.Clear();
                    InfoBox.Text = "Попытка добавления в RGS_Travel_VIP завершилась неудачей. Новая запись не добавлена." +
                                    "Проверьте формат ввода поля < Дата рождения >.";
                }
            }

        }

        private void On_VipBDDelete(object sender, RoutedEventArgs e)
        {
           bool error = false;
            try
            {
                int count = ValidationImplementing();
                if (count > 0)
                {
                    throw new ArgumentException("Заполните отмеченное поле ввода.");
                }
            }
            catch (ArgumentException ex)
            {
                InfoBox.Clear();
                InfoBox.Text = ex.Message;
                error = true;
            }
            if (!error)
            {
                bool result = false;
                string Name = name.Text;
                string BirthDate = birthDate.Text;
                result = VirtuDB.DeleteFromRGS_Travel_VIP(Name, BirthDate);
                if (result)
                {
                    InfoBox.Clear();
                    InfoBox.Text = "Удаление записи прошло успешно";
                }
                else
                {
                    InfoBox.Clear();
                    InfoBox.Text = "Удаление записи завершилось неудачно. Запись не удалена.";
                }
            }
        }

        private void On_FromInsuredListDelete(object sender, RoutedEventArgs e)
        {
            bool error = false;
            try
            {
                int count = ValidationImplementing();
                if (count > 0)
                {
                    throw new ArgumentException("Заполните отмеченное поле ввода.");
                }
            }
            catch (ArgumentException ex)
            {
                InfoBox.Clear();
                InfoBox.Text = ex.Message;
                error = true;
            }
            if (!error)
            {
                string NUMBER = number.Text;
                string SerialBefore = serialBefore.Text;
                string NameToDelete = personToDelete.Text;
                Guid policyID = VirtuDB.PolicyIDGetter(NUMBER, SerialBefore);

                bool result = VirtuDB.FromInsuredListDelete(policyID, NameToDelete);
                if (result)
                {
                    InfoBox.Clear();
                    InfoBox.Text = "Застрахованный был удален из списка.";
                }
                else
                {
                    InfoBox.Clear();
                    InfoBox.Text = "Попытка удаления Застрахованного из списка завершилась неудачей.";
                }
            }
        }

        private void On_VIPPropertyDelete(object sender, RoutedEventArgs e)
        {
            bool error = false;
            try
            {
                int count = ValidationImplementing();
                if (count > 0)
                {
                    throw new ArgumentException("Заполните отмеченное поле ввода.");
                }
            }
            catch (ArgumentException ex)
            {
                InfoBox.Clear();
                InfoBox.Text = ex.Message;
                error = true;
            }
            if (!error)
            {
                string NUMBER = number.Text;
                string SerialBefore = serialBefore.Text;
                string Name = personToDelete.Text;
                Guid policyID = VirtuDB.PolicyIDGetter(NUMBER, SerialBefore);
                bool result = VirtuDB.DeleteVipPropertyInPolicyBody(policyID);
                if (result)
                {
                    InfoBox.Clear();
                    InfoBox.Text = "Удаление признака прошло успешно";
                }
                else
                {
                    InfoBox.Clear();
                    InfoBox.Text = "Попытка удаления признака закончилась неудачей.  Одна из возможных причин: признак в теле полиса не существует.";
                }
            }          
        }

        private void On_VIPPropertyAdd(object sender, RoutedEventArgs e)
        {
            bool error = false;
            try
            {
                int count = ValidationImplementing();
                if (count > 0)
                {
                    throw new ArgumentException("Заполните отмеченное поле ввода.");
                }
            }
            catch (ArgumentException ex)
            {
                InfoBox.Clear();
                InfoBox.Text = ex.Message;
                error = true;
            }
            if (!error)
            {
                string NUMBER = number.Text;
                string SerialBefore = serialBefore.Text;
                string Name = name.Text;
                string BirthDate = birthDate.Text;
                Guid policyID = VirtuDB.PolicyIDGetter(NUMBER, SerialBefore);
                bool result = VirtuDB.AddSingleVipProperty(policyID, Name, BirthDate);
                if (result)
                {
                    InfoBox.Clear();
                    InfoBox.Text = "Добавление признака  прошло успешно";
                }
                else
                {
                    InfoBox.Clear();
                    InfoBox.Text = "Попытка добавления признака закончилась неудачей.";
                }
            }
        }

        private int ValidationImplementing()
        {
            List<string> formText = new List<string>();
            formText.Add(number.Text);
            formText.Add(serialBefore.Text);
            formText.Add(name.Text);
            formText.Add(birthDate.Text);
            List<TextBox> boxes=new List<TextBox>();
            boxes.Add(number);
            boxes.Add(serialBefore);
            boxes.Add(name);
            boxes.Add(birthDate);
            int count = 0;

            for(int i=0; i<formText.Count; i++)
            {
                if(string.IsNullOrEmpty(formText[i]))
                {
                    boxes[i].Background = Brushes.Red;
                    InfoBox.Clear();
                    InfoBox.Text = "Заполните отмеченное поле ввода.";
                    count++;
                }
            }
            return count;
        }

    }
}
