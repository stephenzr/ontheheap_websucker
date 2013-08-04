using System;
namespace Ontheheap
{
    public interface IPageSaver
    {
        void SaveStream(System.IO.Stream data, string name, string contentType, string identifier);
    }
}
