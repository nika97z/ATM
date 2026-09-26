using ATM.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATM.Services
{
    public class LogService
    {
        private readonly Interface1 _repository;

        public LogService(Interface1 repository)
        {
            _repository = repository;
        }

        public void Log(string message)
        {
            if (_repository == null)
            {
                throw new ArgumentNullException(nameof(_repository), "Repository cannot be null.");
            }
            _repository.Log(message);
        }
    }
}
