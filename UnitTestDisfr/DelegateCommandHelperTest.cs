using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using disfr.UI;

namespace UnitTestDisfr
{
    [TestClass]
    public class DelegateCommandHelperTest
    {
        private static readonly Task CompletedTask = Task.FromResult<object>(null);

        [TestMethod]
        public void GetHelp_OneParam_1()
        {
            var m = new DummyViewModel_OneParam_1();
            DelegateCommandHelper.GetHelp(m);

            m.Reset(false);
            m.DummyCommand.CanExecute("abc").Is(false);
            m.DummyCommand_CanExecute_Param.Is("abc");
            m.DummyCommand_Execute_Param.Is("");

            m.Reset(true);
            m.DummyCommand.CanExecute("def").Is(true);
            m.DummyCommand_CanExecute_Param.Is("def");
            m.DummyCommand_Execute_Param.Is("");

            m.Reset(false);
            m.DummyCommand.Execute("ghi");
            m.DummyCommand_CanExecute_Param.Is("");
            m.DummyCommand_Execute_Param.Is("ghi");

            m.Reset(true);
            m.DummyCommand.Execute("jkl");
            m.DummyCommand_CanExecute_Param.Is("");
            m.DummyCommand_Execute_Param.Is("jkl");
        }

        class DummyViewModel_OneParam_1 : DummyViewModelBase
        {
            public DelegateCommand<string> DummyCommand { get; private set; }

            private Task DummyCommand_ExecuteAsync(string dummy)
            {
                DummyCommand_Execute_Param = dummy;
                return CompletedTask;
            }

            private bool DummyCommand_CanExecute(string dummy)
            {
                DummyCommand_CanExecute_Param = dummy;
                return DummyCommand_CanExecute_Value;
            }
        }



        [TestMethod]
        public void GetHelp_OneParam_2()
        {
            var m = new DummyViewModel_OneParam_2();
            DelegateCommandHelper.GetHelp(m);

            m.Reset(false);
            m.DummyCommand.CanExecute("abc").Is(true);
            m.DummyCommand_CanExecute_Param.Is("");
            m.DummyCommand_Execute_Param.Is("");

            m.Reset(true);
            m.DummyCommand.CanExecute("def").Is(true);
            m.DummyCommand_CanExecute_Param.Is("");
            m.DummyCommand_Execute_Param.Is("");

            m.Reset(false);
            m.DummyCommand.Execute("ghi");
            m.DummyCommand_CanExecute_Param.Is("");
            m.DummyCommand_Execute_Param.Is("ghi");

            m.Reset(true);
            m.DummyCommand.Execute("jkl");
            m.DummyCommand_CanExecute_Param.Is("");
            m.DummyCommand_Execute_Param.Is("jkl");
        }

        class DummyViewModel_OneParam_2 : DummyViewModelBase
        {
            public DelegateCommand<string> DummyCommand { get; private set; }

            private Task DummyCommand_ExecuteAsync(string dummy)
            {
                DummyCommand_Execute_Param = dummy;
                return CompletedTask;
            }
        }



        [TestMethod]
        public void GetHelp_OneParam_3()
        {
            var m = new DummyViewModel_OneParam_3();
            AssertEx.Catch<DelegateCommandHelperException>(() => DelegateCommandHelper.GetHelp(m));
        }

        class DummyViewModel_OneParam_3 : DummyViewModelBase
        {
            public DelegateCommand<string> DummyCommand { get; private set; }

            private Task DummyCommand_ExecuteAsync(string dummy)
            {
                return CompletedTask;
            }

            // wrong parameter type
            private bool DummyCommand_CanExecute(int dummy)
            {
                return DummyCommand_CanExecute_Value;
            }
        }



        [TestMethod]
        public void GetHelp_OneParam_4()
        {
            var m = new DummyViewModel_OneParam_4();
            DelegateCommandHelper.GetHelp(m);

            m.Reset(false);
            m.DummyCommand.CanExecute("abc").Is(false);
            m.DummyCommand_CanExecute_Param.Is("abc");
            m.DummyCommand_Execute_Param.Is("");

            m.Reset(true);
            m.DummyCommand.CanExecute("def").Is(true);
            m.DummyCommand_CanExecute_Param.Is("def");
            m.DummyCommand_Execute_Param.Is("");

            m.Reset(false);
            m.DummyCommand.Execute("ghi");
            m.DummyCommand_CanExecute_Param.Is("");
            m.DummyCommand_Execute_Param.Is("ghi");

            m.Reset(true);
            m.DummyCommand.Execute("jkl");
            m.DummyCommand_CanExecute_Param.Is("");
            m.DummyCommand_Execute_Param.Is("jkl");
        }

        class DummyViewModel_OneParam_4 : DummyViewModelBase
        {
            public DelegateCommand<string> DummyCommand { get; private set; }

            // different but assignable parameter type
            private Task DummyCommand_ExecuteAsync(object dummy)
            {
                DummyCommand_Execute_Param = (string)dummy;
                return CompletedTask;
            }

            // different but assignable parameter type
            private bool DummyCommand_CanExecute(object dummy)
            {
                DummyCommand_CanExecute_Param = (string)dummy;
                return DummyCommand_CanExecute_Value;
            }
        }



        [TestMethod]
        public void GetHelp_OneParam_5()
        {
            var m = new DummyViewModel_OneParam_5();
            AssertEx.Catch<DelegateCommandHelperException>(() => DelegateCommandHelper.GetHelp(m));
        }

        class DummyViewModel_OneParam_5 : DummyViewModelBase
        {
            public DelegateCommand<object> DummyCommand { get; private set; }

            private Task DummyCommand_ExecuteAsync(object dummy)
            {
                return CompletedTask;
            }

            // wrong parameter type with reverse assignable combination
            private bool DummyCommand_CanExecute(string dummy)
            {
                return DummyCommand_CanExecute_Value;
            }
        }



        [TestMethod]
        public void GetHelp_OneParam_6()
        {
            var m = new DummyViewModel_OneParam_6();
            AssertEx.Catch<DelegateCommandHelperException>(() => DelegateCommandHelper.GetHelp(m));
        }

        class DummyViewModel_OneParam_6 : DummyViewModelBase
        {
            public DelegateCommand<string> DummyCommand { get; private set; }

            private Task DummyCommand_ExecuteAsync(string dummy)
            {
                return CompletedTask;
            }

            // wrong return type
            private string DummyCommand_CanExecute(string dummy)
            {
                return null;
            }
        }



        [TestMethod]
        public void GetHelp_OneParam_7()
        {
            var m = new DummyViewModel_OneParam_7();
            AssertEx.Catch<DelegateCommandHelperException>(() => DelegateCommandHelper.GetHelp(m));
        }

        class DummyViewModel_OneParam_7 : DummyViewModelBase
        {
            public DelegateCommand<string> DummyCommand { get; private set; }

            // wrong parameter type
            private Task DummyCommand_ExecuteAsync(int dummy)
            {
                return CompletedTask;
            }
        }



        [TestMethod]
        public void GetHelp_OneParam_8()
        {
            var m = new DummyViewModel_OneParam_8();
            AssertEx.Catch<DelegateCommandHelperException>(() => DelegateCommandHelper.GetHelp(m));
        }

        class DummyViewModel_OneParam_8 : DummyViewModelBase
        {
            public DelegateCommand<string> DummyCommand { get; private set; }

            private Task DummyCommand_ExecuteAsync(string dummy)
            {
                return CompletedTask;
            }

            // extra overloaded method
            private void DummyCommand_ExecuteAsync(int dummy)
            {
            }
        }


    }

    abstract class DummyViewModelBase
    {
        public string DummyCommand_Execute_Param;

        public string DummyCommand_Execute_Param2;

        public string DummyCommand_CanExecute_Param;

        public string DummyCommand_CanExecute_Param2;

        public bool DummyCommand_CanExecute_Value;

        public void Reset(bool can_execute_value)
        {
            DummyCommand_Execute_Param = "";
            DummyCommand_Execute_Param2 = "";
            DummyCommand_CanExecute_Param = "";
            DummyCommand_CanExecute_Param2 = "";
            DummyCommand_CanExecute_Value = can_execute_value;
        }
    }
}
