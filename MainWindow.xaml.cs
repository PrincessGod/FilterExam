using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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

namespace WpfApplication1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Dictionary<string, bool> scallt = new Dictionary<string, bool>();
        Dictionary<string, bool>city = new Dictionary<string, bool>();
        Dictionary<string, bool> region = new Dictionary<string, bool>();
        Dictionary<string,Dictionary<string,bool>> all = new Dictionary<string, Dictionary<string, bool>>();
        private List<Customer> customers;

        Filters<Customer> f = new Filters<Customer>();
        FiltersAny<Customer> afs = new FiltersAny<Customer>();
        FiltersAny<Customer> afc = new FiltersAny<Customer>();
        FiltersKeysAny<Customer,string> afr = new FiltersKeysAny<Customer,string>(c=>c.Region);
        public MainWindow()
        {
            InitializeComponent();

            customers = new List<Customer> {
                new Customer {ID ="A",City ="New York",Country ="USA",Region ="North America",Sales =9956},
                new Customer {ID ="B",City ="New York",Country ="USA",Region ="North America",Sales =7777},
                 new Customer {ID ="C",City ="XiAn",Country ="China",Region ="Asia",Sales =7777},
                  new Customer {ID ="D",City ="New York",Country ="USA",Region ="North America",Sales =9999},
                   new Customer {ID ="E",City ="BeiJing",Country ="China",Region ="Asia",Sales =8888},
                    new Customer {ID ="F",City ="New York",Country ="USA",Region ="North America",Sales =8888}
            };

            foreach (var VARIABLE in customers)
            {
                Text.Text += VARIABLE.ToString();
            }

            Text.Text += "\r\n  \r\n";

            //foreach (var VARIABLE in customers.Where(filtering))
            //{
            //    Text.Text += VARIABLE.ToString();
            //}

            var xy = customers.Distinct(d => d.Sales).ToList();


            foreach (var VARIABLE in xy)
            {
                scallt.Add(VARIABLE.Sales.ToString(), true);
                afs.Conditions.Add(new FilterBase<Customer, string>(c => c.Sales.ToString(), VARIABLE.Sales.ToString()));
            }

            var cy = customers.Distinct(d => d.City).ToList();
            foreach (var VARIABLE in cy)
            {
                city.Add(VARIABLE.City, true);
                afc.Conditions.Add(new FilterBase<Customer, string>(c => c.City, VARIABLE.City));
            }

            var cr = customers.Distinct(d => d.Region).ToList();
            foreach (var VARIABLE in cr)
            {
                afr.Keys.Add(VARIABLE.Region);
                region.Add(VARIABLE.Region, true);
            }

            all.Add("scall",scallt);
            all.Add("city",city);

            listb.ItemsSource = all["scall"];
            listC.ItemsSource = all["city"];
            listR.ItemsSource = afr.Keys;
            f.Conditions.Add(afs);
            f.Conditions.Add(afc);
            f.Conditions.Add(afr);

            var xxx = customers.Where(f.Match).ToList();

        }

        private bool filtering(Customer c)
        {
            return ((c.ID == "A" || c.ID == "F") && c.Region == "North America");
        }

        private bool filteringScall(Customer c)
        {
            foreach (var VARIABLE in scallt.Where(x => x.Value==true))
            {
                if (c.Sales.ToString() == VARIABLE.Key)
                    return true;
            }

            return false;
        }

        private bool filteringCity(Customer c)
        {
            foreach (var VARIABLE in city.Where(x => x.Value == true))
            {
                if (c.City == VARIABLE.Key)
                    return true;
            }

            return false;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox x = (CheckBox) sender;
            if (scallt.ContainsKey(x.Content.ToString()))
            {
                scallt[x.Content.ToString()] = (bool)x.IsChecked;
                var xx =  afs.Conditions.Find(c => c.Match(new Customer() {Sales = int.Parse(x.Content.ToString())}));
                if (x.IsChecked == true)
                {
                    if (xx == null)
                    {
                        afs.Conditions.Add(new FilterBase<Customer,string>(o => o.Sales.ToString(), x.Content.ToString()));
                    }
                }
                if (x.IsChecked == false)
                {
                    afs.Conditions.Remove(xx);
                }
            }
            if (city.ContainsKey(x.Content.ToString()))
            {
                city[x.Content.ToString()] = (bool)x.IsChecked;
                var xx = afc.Conditions.Find(c => c.Match(new Customer() { City = x.Content.ToString() }));
                if (x.IsChecked == true)
                {
                    if (xx == null)
                    {
                        afc.Conditions.Add(new FilterBase<Customer, string>(o => o.City.ToString(), x.Content.ToString()));
                    }
                }
                if (x.IsChecked == false)
                {
                    afc.Conditions.Remove(xx);
                }
            }
            if (region.ContainsKey(x.Content.ToString()))
            {
                var xx = afr.Keys.Contains(x.Content.ToString());
                if (x.IsChecked == true)
                {
                    if (!xx)
                    {
                        afr.Keys.Add(x.Content.ToString());
                    }
                }
                if (x.IsChecked == false)
                {
                    if (xx)
                    {
                        afr.Keys.Remove(x.Content.ToString());
                    }
                }
            }


            GetResult();
        }

        private void GetResult()
        {
            Text.Text += "\r\n  \r\n";

            foreach (var VARIABLE in customers.Where(f.Match))
            {
                Text.Text += VARIABLE.ToString();
            }

        }
    }

    public interface IFilter<T>
    {
        bool Match(T l);
    }
    public class FilterBase<T,V>:IFilter<T>
    {
        public FilterBase(Func<T, V> vo, V ve)
        {
            velue = vo;
            march = ve;
        }
        private Func<T, V> velue;

        private V march;

        public V Key { get { return march; } }

        public bool Match(T l)
        {
            return EqualityComparer<V>.Default.Equals(velue(l), march);
        }
    }

    /// <summary>
    /// 满足任何一个条件即可
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FiltersAny<T>:IFilter<T>
    {
        public List<IFilter<T>> Conditions;
        public FiltersAny()
        {
            Conditions = new List<IFilter<T>>();
        }
        public FiltersAny(List<IFilter<T>> c )
        {
            Conditions = c;
        }

        public bool Match(T l)
        {
            foreach (var VARIABLE in Conditions)
            {
                if (VARIABLE.Match(l))
                {
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// 满足所有条件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Filters<T> : IFilter<T>
    {
        public List<IFilter<T>> Conditions;

        public Filters()
        {
            Conditions = new List<IFilter<T>>();
        }

        public Filters(List<IFilter<T>> l)
        {
            Conditions = l;
        }
        public bool Match(T l)
        {
            foreach (var VARIABLE in Conditions)
            {
                if (!VARIABLE.Match(l))
                    return false;
            }
            return true;
        }
    }
    /// <summary>
    /// 满足任一条件但是可以只提供键值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="V"></typeparam>
    public class FiltersKeysAny<T, V> : IFilter<T>
    {
        public List<V> Keys;
        private Func<T, V> value;

        public FiltersKeysAny(Func<T, V> f)
        {
            Keys = new List<V>();
            value = f;
        }

        public FiltersKeysAny(Func<T, V> f, List<V> l)
            : this(f)
        {
            Keys = l;
        }
        public bool Match(T l)
        {
            foreach (var VARIABLE in Keys)
            {
                var c = new FilterBase<T,V>(value, VARIABLE);
                if (c.Match(l))
                    return true;
            }
            return false;
        }
    }

    class Customer
    {
        public string ID { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }
        public int Sales { get; set; }

        public override string ToString()//重写ToString(),默认的ToString()仅输出类型名称
        {
            return "ID:" + ID + "City:" + City + "Country:" + Country + "Region:" + Region + "Sales:" + Sales + "\r\n";
        }
    }

    public class CommonEqualityComparer<T, V> : IEqualityComparer<T>
    {
        private Func<T, V> keySelector;

        public CommonEqualityComparer(Func<T, V> keySelector)
        {
            this.keySelector = keySelector;
        }

        public bool Equals(T x, T y)
        {
            return EqualityComparer<V>.Default.Equals(keySelector(x), keySelector(y));
        }

        public int GetHashCode(T obj)
        {
            return EqualityComparer<V>.Default.GetHashCode(keySelector(obj));
        }
    }

    public static class DistinctExtensions
    {
        public static IEnumerable<T> Distinct<T, V>(this IEnumerable<T> source, Func<T, V> keySelector)
        {
            return source.Distinct(new CommonEqualityComparer<T, V>(keySelector));
        }
    }


}
