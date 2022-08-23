# Schedule Server

<div  align=center>
    <img src="https://github.com/HIT-ReFreSH/HitGeneralServices/raw/master/images/Full_2048.png" width = 30% height = 30%  />
</div>

![DockerHub-v](https://img.shields.io/docker/v/ferdinandsu/scheduleserver/latest?style=flat-square)
![DockerHub-DL](https://img.shields.io/docker/pulls/ferdinandsu/scheduleserver?style=flat-square)
![DockerHub-size](https://img.shields.io/docker/image-size/ferdinandsu/scheduleserver?style=flat-square)
![GitHub](https://img.shields.io/github/license/HIT-ReFreSH/ScheduleServer?style=flat-square)
![GitHub last commit](https://img.shields.io/github/last-commit/HIT-ReFreSH/ScheduleServer?style=flat-square)
![GitHub repo size](https://img.shields.io/github/repo-size/HIT-ReFreSH/ScheduleServer?style=flat-square)
![GitHub code size](https://img.shields.io/github/languages/code-size/HIT-ReFreSH/ScheduleServer?style=flat-square)

[View at DockerHub](https://hub.docker.com/repository/docker/ferdinandsu/scheduleserver)

适用于哈尔滨工业大学本科/研究生的课程表服务，可以使用docker部署到您的服务器上。

## 使用方法

### 创建配置文件

使用如下格式的配置文件：

```json
{
  "Subscriptions": [
    {
      "Name": "MySchedule",
      "WeekIndex": true,
      "Entries": [
        {
          "Notification": -1,
          "Prefix": "",
          "StudentId": ""
        }
      ],
      "Secret": "Jinitaimei"
    }
  ]
}
```

显然，您可以添加多份Subscription;对于每一份Subscription，Name是它的名字，Secret是它的密码，WeekIndex表示是否启用周数标记，每个订阅可以添加多个Entry。
Entry可以设定通知时间Notification(-1不显示)、前缀Prefix和学号StudentId。
请将编写好的`config.json`放到您的服务器上，注意使用UTF-8编码。

### 部署

建议使用docker部署，请替换您的配置文件位置和端口号。**记得打开防火墙。**

```bash
docker pull ferdinandsu/scheduleserver:latest
docker run -it --name schedule -v /root/schedule.json:/app/config.json -p 10086:80 -d docker.io/ferdinandsu/scheduleserver:latest
```

### 使用订阅

访问以下格式的链接来使用订阅：

```txt
http://ip:port/<Name>?secret=<Secret>
```