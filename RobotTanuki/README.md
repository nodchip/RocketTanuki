# RobotTanuki
USI対応将棋思考エンジン

# 新しいプロジェクトを始める場合

- .NETのプロジェクトを作る
  ```bash
  # new directory内で
  dotnet new console
  ```
# buildする
- プロジェクトファイル(.csproj)が出来たらビルドしてみる
  ```bash
  dotnet build
  ```
- dotnet publishで公開
  ```bash
  dotnet publish -c Release -r win-x64 --self-contained true -o build
  ```