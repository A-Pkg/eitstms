# 使用 .NET 10 SDK 镜像作为编译阶段
# SDK 镜像包含 dotnet restore、build、publish 等编译工具
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# 设置当前阶段的工作目录为 /src，不存在时会创建
# 后续 RUN 在此目录执行，COPY 的相对目标路径也以此为基础
# COPY 的来源路径仍然相对于本机的构建上下文
WORKDIR /src

# 先只复制项目文件，这样可以利用 Docker 构建缓存
# COPY ["来源", "目标"]，"./"也就是"/src/"
# 如果 .csproj 没变化，NuGet restore 结果可以直接复用
COPY ["EITSTMS.csproj", "./"]

# 根据项目文件还原 NuGet 依赖包
RUN dotnet restore "EITSTMS.csproj"

# 复制当前项目目录中的全部源码到容器的 /src
# 包括 Controllers、Models、Views、Program.cs 等
#第一个 .	本机的构建上下文，经过 .dockerignore 筛选
#第二个 .	当前阶段的工作目录 /src
COPY . .

# 以 Release 模式发布项目
# 发布后的文件输出到 /app/publish
RUN dotnet publish "EITSTMS.csproj" -c Release -o /app/publish


# 使用更轻量的 ASP.NET Core 运行时镜像作为最终运行阶段
# 最终镜像不包含完整 SDK，只保留运行项目所需的环境
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

# 设置最终容器的工作目录为 /app
WORKDIR /app

# 声明容器中的 ASP.NET Core 应用使用 8080 端口
# EXPOSE 只是声明端口，不负责把端口映射到宿主机
EXPOSE 8080

# 从前面的 build 阶段复制发布结果到当前 /app 目录
# 最终镜像只保留发布后的文件，不包含源码和 SDK
#--from=build	来源是前面叫 build 的阶段
#/app/publish	第一个阶段中的发布目录
#.	当前阶段的工作目录，即 /app
#它复制的是发布目录中的内容。复制完成后，程序文件位于 /app/EITSTMS.dll。
COPY --from=build /app/publish .

# 设置容器启动时执行的命令
# 相当于在容器中执行：dotnet EITSTMS.dll
ENTRYPOINT ["dotnet", "EITSTMS.dll"]