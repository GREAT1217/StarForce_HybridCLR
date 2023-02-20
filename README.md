## 简介

使用 HybridCLR 对 StarForce 实现热更新。将 StarForce 的游戏逻辑剔除，可以得到一个接入了 HybridCLR 的 GameFramework。可 fork 此仓库使用 👉  [UnityGameFramework](https://github.com/GREAT1217/UnityGameFramework) 

### 更新日志

> 2022年06月	演示视频  [bilibili](https://www.bilibili.com/video/BV1wB4y1Q7JK)（主要演示 GameFramework 的热更流程）

> 2022年07月	增加适用于 GameFramework 的 HybridCLR 工具 HybridCLRBuilder 。

> 2022年11月	HybridCLR 官方的的工具流已经完善，移除了 HybridCLRBuilder 中的安装模块、桥接函数模块。主要使用 Build 模块。

> 后续的更新，只需要更新 HybridCLR，除了 API 的变化，基本不需要更新此项目。如有问题，欢迎在 Issue 提问。

### 适用于 GameFramework 的工具

HybridCLR 官方的的工具流已经完善，在接入 GameFramework 时唯一需要扩展的就是：将热更新的 dll 文件，拷贝至 Assets 目录下，用于 Resource 模块资源编辑与打包。已在此工具的 Build 模块实现。

![编辑器工具](https://gitee.com/great1217/cdn/raw/master/images/HybridCLR_Builder.png)

### GameFramework 热更新流程图

![游戏流程图](https://gitee.com/great1217/cdn/raw/master/images/StarForce_Procedure.png)

## 鸣谢

**GameFramework** - [https://gameframework.cn/](https://gameframework.cn/)

**HybridCLR** - [https://focus-creative-games.github.io/hybridclr/](https://focus-creative-games.github.io/hybridclr/)