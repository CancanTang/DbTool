namespace DbTool.ViewModels;

public sealed record ServiceCommandDescriptor<T>(string Label, T Service);
