# 涂鸦显示系统使用说明

## 🎯 系统功能

这个系统实现了通过UDP信号控制涂鸦图片显示的功能：

1. **UDP接收** - 监听UDP消息，将消息内容作为涂鸦文件名
2. **文件加载** - 从F:\Graffiti文件夹加载对应的PNG/JPG图片
3. **Canvas循环** - 在Canvas1、Canvas2、Canvas3上轮流显示涂鸦
4. **数量控制** - 最多显示100个涂鸦，超出时自动替换最早的

## 🚀 使用方法

### 1. 场景设置

确保场景中有以下组件：
- `GraffitiManager` - 涂鸦管理器
- `CanvasController` x3 - 三个Canvas控制器
- `UDPManager` - UDP通信管理器

### 2. GraffitiManager配置

在Inspector中设置：
- `canvasControllers` - 拖入3个Canvas控制器
- `graffitiPrefab` - 涂鸦显示预制体
- `maxGraffitiCount` - 最大涂鸦数量（默认100）

### 3. CanvasController配置

每个Canvas控制器设置：
- `graffitiScale` - 涂鸦缩放比例
- `graffitiSpeed` - 涂鸦移动速度
- `graffitiYCenter` - Y轴中心位置
- `graffitiYRange` - Y轴随机范围

### 4. UDPManager配置

设置网络参数：
- `localPort` - 本地监听端口
- `remoteIp` - 远程目标IP
- `remotePort` - 远程目标端口

## 📡 UDP消息格式

### 涂鸦文件名消息
直接发送文件名（不需要扩展名）：
```
test1
sample
demo
```

系统会自动尝试以下扩展名：
- .png
- .PNG
- .jpg
- .JPG
- .jpeg
- .JPEG

### 列车信号消息（JSON格式）
```
{"type":"train","color":"red"}
```

## 🎨 涂鸦显示逻辑

### Canvas循环顺序
1. **Canvas1** - 优先显示新涂鸦
2. **Canvas2** - Canvas1满时使用
3. **Canvas3** - Canvas1和2都满时使用
4. **回到Canvas1** - 当有Canvas有空位时

### 数量控制
- 每个Canvas最多33个涂鸦
- 总共最多100个涂鸦
- 超出时自动移除最旧的涂鸦

## 🐛 常见问题解决

### 1. 涂鸦不显示
- 检查F:\Graffiti文件夹是否存在
- 确认文件名是否正确
- 查看Console日志信息

### 2. UDP消息不接收
- 检查端口是否被占用
- 确认防火墙设置
- 验证网络连接

### 3. Canvas控制器错误
- 确保3个Canvas控制器都已设置
- 检查Canvas的RectTransform组件
- 验证Canvas的层级设置

## 🔧 调试功能

### UDPManager调试按钮
- **检查场景设置** - 验证所有组件状态
- **测试涂鸦功能** - 手动测试涂鸦加载
- **发送测试消息** - 测试UDP通信
- **发送测试涂鸦文件名** - 测试涂鸦文件加载

### 日志信息
系统会输出详细的日志信息：
- UDP消息接收状态
- 文件加载过程
- Canvas分配情况
- 涂鸦数量统计

## 📁 文件结构

```
F:\Graffiti\
├── test1.png
├── sample.jpg
├── demo.png
└── ...
```

## ⚠️ 注意事项

1. **文件路径** - 确保F:\Graffiti文件夹存在且有读取权限
2. **图片格式** - 支持PNG、JPG、JPEG格式
3. **文件名** - 避免使用特殊字符和空格
4. **性能** - 大量涂鸦可能影响性能，建议适当调整maxGraffitiCount
5. **内存** - 图片会自动缓存，注意内存使用

## 🎮 测试步骤

1. 在F:\Graffiti文件夹放入测试图片
2. 运行Unity场景
3. 使用UDP工具发送文件名（如：test1）
4. 观察涂鸦是否正确显示在Canvas上
5. 检查Console日志确认系统状态

## 📞 技术支持

如果遇到问题，请检查：
1. Console日志输出
2. 组件配置状态
3. 文件路径和权限
4. 网络连接状态


