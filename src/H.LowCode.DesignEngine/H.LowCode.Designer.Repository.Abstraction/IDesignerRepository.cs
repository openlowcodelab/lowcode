using System;

namespace H.LowCode.Designer.Repository.Abstraction
{
    public interface IDesignerRepository
    {
        void SaveMetadata(string jsonSchema);
    }
}