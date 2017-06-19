# EFFC.General.Application
本项目为EFFC的通用应用架构，该项目提供了如何使用的sample

# 使用方法
拿到工程源码后，查看Web.config
* dbconn：默认访问db的链接串
* mongodb：mongodb的访问链接，EFFC有提供Mongodb的访问方法，如果没有配置mongodb则不用管该参数，或者删除
* DebugMode：是否为debug模式，框架中通过该标记区分生成和开发环境，出错的提示信息是不同；另外debug模式对于hostjs开发很重要，开启debug模式远程EFFC.HostDevelop才能访问应用服务进行远程hostjs开发
* Machine_No:机器码，log记录中用于标记是哪台应用的错误记录，用于分布式部署时的识别
* Count_per_Page：分页参数，每页多少笔记录
* FrameStartPage：系统默认起始页面，不设置则自动识别为根目录下的index.html
* HostJs_Path：HostJs的代码路径，如果启用hostjs则需要配置该参数，在本项目的目录中有提供HostJs的目录，将该参数指向HostJs目录的绝对路径即可使用
* weixin_token,weixin_encry_key,weixin_Appid,weixin_Appsecret:微信相关参数，此处不做详细说明

# 第一次运行
第一次运行时，将上述参数配置好之后，编译无错即可运行，默认的的起始页面为~/sample.razor.go（~为根路径），该sample的处理逻辑在Web.Business/Sample/SampleLogic.cs,razor为action识别，SampleLogic.cs中识别为RazorSample这个方法。
访问sample.go,则访问的是Load方法，Load方法中有提供DB一类的基本操作方法（已经注释掉了，如果需要可以在配好db连接和调整对应的sql后即可放开）

EFFC框架下的访问都是rest的访问方式，不同的是采用L.A请求（Logic.action）来访问不同的资源与后台接口，L.A请求是忽略路径的，/sample.razor.go与/xxxx/sample.razor.go是相同的，另外本版本中有提供类似路由的请求方式，如/sample/razor就是/sample.razor.go

# 目录结构
分为4个部分
* Builder.Web:应用的建构层，基于EFFC底层框架，构建符合系统业务标准的框架，该层定义了go和hgo请求，并对Logic层做了扩展，如果有自己的业务要求或对外接口定义可以在本结构中定义
* Unit:单元层，改层定义了DB的访问，如果要编写访问DB的sql或sp则在改层定义即可，相关的sample可以查看SampleUnit.cs
* web：站点，启动项目，系统的页面，js，css都放在改目录中
* Web.Business:业务逻辑层，相关sample可以查看SampleLogic.cs

# 框架的运行流程和生命周期
一个基本的go请求层次如下：
* http请求->FrameHandlerFactory->GoHandler->GoLogic->Unit
如果想要对请求方式进行路由扩展，可以在FrameHandlerFactory进行修改处理，L.A的请求基本是分辨出请求中的Logic和action，跟采用何种路由或编写方式都不做约定，可以自行修改

GoHandler定义了go请求的处理流程和生命周期以及错误处理，如果想进行生命周期的修改和调整可以在此处进行

GoLogic是进行业务逻辑处理的地方，如果需要在业务逻辑处理之前进行某项公共作业可以在GoLogic中调整

Unit是数据访问的定义，如果想对访问方式和sql进行一些预处理则在BaseDBUnit中调整


