using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using CodeSmith.Engine;
using System.Text.RegularExpressions;
using System.Data;
using SchemaExplorer;
using System;
using CodeSmith.BaseTemplates;
using CodeSmith.CustomProperties;


public class CodeBehindClass : CodeTemplate
{
    public CodeBehindClass(){
        
    }
    
	private String _nameSpace = "com.wmqe.pms";
    private String _moduleName = "";
    private String _fullNameSpace = "";
    private String _author = "";
    private String _tablePrefix = "tb_pr_,tb_g_,tb_se_,tb_sp_,tb_";
    private String _deleteFlag = "DELETEFLAG"; // 逻辑删除字段的名称
    private String _version = "VERSION"; // 乐观锁字段的名称，目前不支持
    
    //public CodeSmith.Engine.MapCollection JavaAlias{get;set;}
    
    private String mapperJavaPath = "dao"; //mapper
    private String mapperXmlPath = "dao.conf"; //mapper.conf
    private String modelPath = "model";
    private String servicePath = "service";
    private String serviceTestPath = "service.test";
    private String mapperXmlPostfix = ".ibatis.xml";// "Mapper.xml";
    
    
    protected String DeleteFlagValue{
        get
        {
            return "1";
        }        
    }
    
    protected String UnDeleteFlagValue{
        get
        {
            return "0";
        }        
    }
    
    protected String VersionColName{
        get
        {
            return _version;
        }        
    }
    
    
    protected String MapperXmlPostfix{
        get
        {
            return mapperXmlPostfix;
        }        
    }
    
    protected String MapperJavaPath{
        get
        {
            return OutputDirectory + "\\main\\" + _fullNameSpace.Replace(".","\\") + "\\" + mapperJavaPath.Replace(".","\\");
        }        
    }
    
    protected String MapperJavaPackage{
        get
        {
            return _fullNameSpace.TrimEnd('.') + "." + mapperJavaPath;
        }        
    }
    
    
    protected String MapperXmlPath{
        get
        {
            return OutputDirectory + "\\main\\" + _fullNameSpace.Replace(".","\\") + "\\" + mapperXmlPath.Replace(".","\\");
        }        
    }
    
    protected String MapperXmlPackage{
        get
        {
            return _fullNameSpace.TrimEnd('.') + "." + mapperXmlPath;
        }        
    }
    
    protected String ModelPath{
        get
        {
            return OutputDirectory + "\\main\\" + _fullNameSpace.Replace(".","\\") + "\\" + modelPath.Replace(".","\\");
        }        
    }
    
    protected String ModelPackage{
        get
        {
            return _fullNameSpace.TrimEnd('.') + "." + modelPath;
        }        
    }
    
     protected String ServicePath{
        get
        {
            return OutputDirectory + "\\main\\" + _fullNameSpace.Replace(".","\\") + "\\" + servicePath.Replace(".","\\");
        }        
    }
    
    protected String ServicePackage{
        get
        {
            return _fullNameSpace.TrimEnd('.') + "." + servicePath;
        }        
    }
    
    
    protected String ServiceTestPath{
        get
        {
            return OutputDirectory + "\\test\\" + _fullNameSpace.Replace(".","\\") + "\\" + serviceTestPath.Replace(".","\\");
        }        
    }
    
    protected String ServiceTestPackage{
        get
        {
            return _fullNameSpace.TrimEnd('.') + "." + serviceTestPath;
        }        
    }
    
	[Category("01.项目信息")]
	[Description("包名")]
	public virtual String NameSpace
	{
		get {return _nameSpace;}
		set {_nameSpace = value;}
	}
    
    [Category("01.项目信息")]
	[Description("模块名")]
	public virtual String ModuleName
	{
		get {return _moduleName;}
		set {_moduleName = value;}
	}
    
    //[Category("01.项目信息")]
	//[Description("全包名")]
	public virtual String FullNameSpace
	{
		get {return _fullNameSpace;}
		set {_fullNameSpace = value;}
	}
	
	[Category("01.项目信息")]
	[Description("作者")]
    //[Optional]
	public virtual String Author
	{ 
		get {return _author;}
		set {_author = value;}
	}
    
    [Category("02.数据库")]
	[Description("表的前缀")]
	public virtual String TablePrefix
	{ 
		get {return _tablePrefix;}
		set {_tablePrefix = value;}
	}
    
    
    private String srcDir = @".\output";

    [Editor(typeof(System.Windows.Forms.Design.FolderNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public virtual string OutputDirectory
    {
          get {return srcDir;}
          set {srcDir= value;}
    }
    
    public bool IsIdentity(ColumnSchema col){
        if(col == null)
            return false;   
        bool isIdentity = Boolean.Parse(col.ExtendedProperties["CS_IsIdentity"].Value.ToString());
        return isIdentity;
    }
    
        
    public string GetJdbcType(ColumnSchema col) {
        try{       
            
            string val = col.NativeType.ToString().ToUpper();
            switch(val){
                case "INT" : return "INTEGER";
				case "DATE" : return "DATE";
				case "DATETIME" : return "TIMESTAMP";  //return "DATE";
                default:
                    return val;
            }
            
            //return JavaAlias[col.NativeType];
        }catch(Exception ex){
            //Response.Write(col.NativeType.ToString()+ex.Message);
            return "["+ex.Message.ToString()+"]";
        }
    }
    
    /*
    获取逻辑删除的列
    */
    public ColumnSchema GetDeleteFlagColumn(TableSchema table){
       ColumnSchema deleteFlagCol = null;
        foreach(ColumnSchema col in table.Columns) {  
            for(int i=0;i<col.ExtendedProperties.Count;i++){
                if(col.Name.ToUpper().Equals(_deleteFlag.ToUpper())){
                    deleteFlagCol = col;
                    break;
                }
                //Response.WriteLine(col.FullName + " pk:" + col.IsPrimaryKeyMember + ":" + col.ExtendedProperties[i].Name + "=" + col.ExtendedProperties[i].Value);
            }
            if(deleteFlagCol != null)
                break;
        }
        return deleteFlagCol;  
    }
    
    public List<ColumnSchema> GetUniqueColumns(TableSchema table){
        List<ColumnSchema> list = new List<ColumnSchema>();
         foreach(ColumnSchema col in table.Columns) {  
             if(col.IsUnique && !col.IsPrimaryKeyMember)
                 list.Add(col);
         }
        return list;
    }
    
    public ColumnSchema GetPrimaryKey(TableSchema table){
        ColumnSchema pkCol = null;
        foreach(ColumnSchema col in table.Columns) {  
            for(int i=0;i<col.ExtendedProperties.Count;i++){
                if(col.IsPrimaryKeyMember){
                    pkCol = col;
                    break;
                }
                //Response.WriteLine(col.FullName + " pk:" + col.IsPrimaryKeyMember + ":" + col.ExtendedProperties[i].Name + "=" + col.ExtendedProperties[i].Value);
            }
            if(pkCol != null)
                break;
        }
        return pkCol;
    }
    
    public void GenerateIndent(int indentLevel)
    {
        for (int i = 0; i < indentLevel; i++)
    	{
    		Response.Write('\t');
    	}
    }
    
    public String GetIndent(int indentLevel)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < indentLevel; i++)
    	{
    		sb.Append('\t');
    	}
        return sb.ToString();
    }
    
    public virtual string GetPascalCaseName(TableSchema table) {
        return StringUtil.ToPascalCase(TrimTablePerfix(table.Name));
    }
    
    public virtual string GetPascalCaseName(ColumnSchema col) {
        return StringUtil.ToPascalCase(col.Name);
    }
    
    public string TrimTablePerfix(string tableName){
        string[] tablePrefixs = TablePrefix.ToLower().Split(',');
        for(int i=0; i<tablePrefixs.Length; i++){
            string tablePrefix = tablePrefixs[i];
            if(tableName.ToLower().StartsWith(tablePrefix)){
                return tableName.Substring(tablePrefix.Length);
            }
        }
        return tableName;
    }
    
    public virtual string GetCamelCaseName(TableSchema table) {
        return StringUtil.ToCamelCase(TrimTablePerfix(table.Name));
    }
    
    public virtual string GetCamelCaseName(ColumnSchema col) {
        return StringUtil.ToCamelCase(col.Name);
    }
  
    
    public void RenderToFile(CodeTemplate codeTemplate, String filename){
        codeTemplate.RenderToFile(filename, true);
    }   
    
    
    public void RemoveUtf8Bom(string path)
    {
        //string path = @"E:\MyProjects\wmqe\svn-project\pms\tools\codesmith-mybatis\output";

        List<string> list = new List<string>();
        FindFile(path, list) ;
        RemoveUtf8Bom(list);
    }


    public void RemoveUtf8Bom(List<string> list)
    {
        foreach (var filename in list)
        {
            string bakFile = filename + ".bak";
            System.IO.File.Move(filename, bakFile);
            System.Text.UTF8Encoding utf8 = new System.Text.UTF8Encoding(false);
            File.WriteAllText(filename, File.ReadAllText(bakFile), utf8);
            System.IO.File.Delete(bakFile);
        }         
    }

    public void FindFile(string dir, List<string> list)
    {
        DirectoryInfo d = new DirectoryInfo(dir);
        FileSystemInfo[] fsinfos = d.GetFileSystemInfos();
        foreach (FileSystemInfo fsinfo in fsinfos)
        {
            if (fsinfo is DirectoryInfo)     //判断是否为文件夹  
            {
                FindFile(fsinfo.FullName, list);//递归调用  
            }
            else
            {
                string filename = fsinfo.FullName.ToLower();
                if(filename.EndsWith(".java") || filename.EndsWith(".xml"))
                    list.Add(fsinfo.FullName);//输出文件的全部路径  
            }
        }
    }
    
 /*
    private OutputFile GetOutputFile(string fileName, string dependentUpon, params object[] metaData)
    {
    	OutputFile outputFile = new OutputFile(fileName);
    	
    	if(!String.IsNullOrEmpty(dependentUpon))
    		outputFile.DependentUpon = Path.GetFullPath(dependentUpon);
    	
    	if(metaData.Length % 2 != 0)
    		throw new Exception("Invalid Metadata: Provide 2 objects per entry, a String (key) followed by an Object.");
    	for(int x=0; x<metaData.Length; x+=2)
        	outputFile.Metadata.Add(metaData[x].ToString(), metaData[x+1]);
    		
    	return outputFile;
    }


    private string GetPageName(TableSchema table)
    {
        string pn=table.Name;
        pn=pn.Replace(TablePrefix,"").ToLower();
        
        return pn;
    }



    private string GetClassName(TableSchema table)
    {   
        //if(TablePrefix!= null && table.Name.StartsWith(this.TablePrefix))    
        //    return table.Name.Remove(0,this.TablePrefix.Length);
        //else
        //    return table.Name;
        
        
        if(table==null)
    	{
    	    return null;
    	}
        string[] fnl=table.Name.Split('_');

        if(fnl.Length==1)
    	    return GetPasalCaseName(table.Name);
    	else
        {
            string fn=string.Empty;
            for(int i=0;i<fnl.Length;i++){
                fn+=GetPasalCaseName(fnl[i]);
            }
            return fn;
        }
    }
    private string GetFolder(string pack)
    {
    	//if (folder.Contains(".") && !folder.EndsWith("."))
        //    folder = folder.Substring(folder.LastIndexOf('.')+1);

        string folder = Path.Combine(SrcDir,pack.Replace(".","/"));
        
        //folder=SrcDir;
        
    	if(String.IsNullOrEmpty(folder))
    		folder = String.Empty;
    	else
    	{
    		if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
    			
    		if (!folder.EndsWith("\\"))
    			folder = String.Format("{0}\\", folder);
    	}
        
    	return folder;
    }
    public string GetPasalCaseName(string value)
    {
    	return value.Substring(0, 1).ToUpper() + value.Substring(1).ToLower();
    }
    */
}