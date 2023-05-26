using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AutoTricklerGui
{
    
    class ScaleData : INotifyPropertyChanged
    {
        private List<decimal> _scaleValues = new List<decimal>();
        private decimal _currentScaleValue = 0.00M;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public decimal CurrentScaleValue {
            get { return _currentScaleValue; }
            set { 
                _currentScaleValue = value; 
                NotifyPropertyChanged();
                NotifyPropertyChanged("CurrentScaleValueString");
            }
        }

        public string CurrentScaleValueString
        {
            get { return _currentScaleValue.ToString() + " grs"; }
            private set { }
        }


        public void addScaleValue(decimal value) {
            _scaleValues.Add(value);
            NotifyPropertyChanged("NumberOfWeighings");
            NotifyPropertyChanged("MinWeight");
            NotifyPropertyChanged("MaxWeight");
            NotifyPropertyChanged("ExtremeSpread");
            NotifyPropertyChanged("AverageWeight");
            NotifyPropertyChanged("StandardDeviation");
        }

        public void ResetScaleValueList() { 
            _scaleValues.Clear(); 
        }

        public int NumberOfWeighings {
            get { return _scaleValues.Count; } 
            private set { }
        }

        public decimal MinWeight { 
            get { return _scaleValues.Count > 0 ? _scaleValues.Min() : 0.00M ; }
            private set { }
        }
        public decimal MaxWeight { 
            get { return _scaleValues.Count > 0 ? _scaleValues.Max() : 0.00M; }
            private set { }
        }
        public decimal ExtremeSpread {
            get { return MaxWeight - MinWeight; }
            private set { }
        }
        public decimal AverageWeight {
            get { return _scaleValues.Count > 0 ? (_scaleValues.Sum() / _scaleValues.Count) : 0.00M; }
            set { }
        }

        public decimal StandardDeviation {
            get { 
                List<decimal> result = new List<decimal>();
                var avg = AverageWeight;
                foreach (var item in _scaleValues)
                {
                    var diff = item - avg;
                    result.Add(diff*diff);
                }
            
                return _scaleValues.Count > 0 ? Convert.ToDecimal(Math.Sqrt(Decimal.ToDouble(result.Sum() / result.Count))) : 0.00M ;
            }
            private set { }
        }


    }
}
