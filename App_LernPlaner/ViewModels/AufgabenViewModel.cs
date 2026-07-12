using App_LernPlaner.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_LernPlaner.ViewModels
{
    public class AufgabenViewModel : INotifyCollectionChanged

    {
        private readonly TaskService _taskService;
        private readonly int _UserId;

      
        public AufgabenViewModel()
        {


        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

       
    }
}
