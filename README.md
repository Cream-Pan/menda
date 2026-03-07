<div id="top"></div>

## 使用技術一覧

<p style="display: inline">
  <img src="https://img.shields.io/badge/-Unity-000000.svg?logo=unity&style=for-the-badge">
  <img src="https://img.shields.io/badge/-C%23-239120.svg?logo=c-sharp&style=for-the-badge&logoColor=white">
  <img src="https://img.shields.io/badge/-Netlify-00C7B7.svg?logo=netlify&style=for-the-badge&logoColor=white">
  <img src="https://img.shields.io/badge/-GitHub-181717.svg?logo=github&style=for-the-badge">
</p>

## 目次

1. [プロジェクトについて](#プロジェクトについて)
2. [環境](#環境)
3. [ディレクトリ構成](#ディレクトリ構成)
4. [主な機能](#主な機能)
5. [設計における工夫と考察](#設計における工夫と考察)
6. [開発者情報](#開発者情報)

## プロジェクト名

麺打 (Menda)

## プロジェクトについて

「シュルレアリスム」をテーマに、現代の無意識的なタイピング習慣に一石を投じるPC向けローマ字タイピング練習アプリです．
大学4年次の研究室ハッカソンにて、4名チームで約1週間という短期間で開発しました．私は主にゲーム設計、基盤システム構築、および外部データ処理を担当しました．

現代の「自動化された無意識的なタイピング」という習慣をあえて崩すことで、プレイヤーに新しい認知体験を提供することを目指しました．シュルレアリスムの思想をヒントに、固定観念を破る「思考の再構築」を促す手段としてタイピングに着目しています．

[▶ 作品を体験する（Netlify）](https://men-da.netlify.app/)

<p align="right">(<a href="#top">トップへ</a>)</p>

## 環境

| 項目                 | 内容                               |
| -------------------- | ---------------------------------- |
| 開発エンジン         | Unity 6000.0.24f1 (または使用バージョン) |
| 言語                 | C#                                 |
| 配信プラットフォーム | WebGL (Netlify)                    |
| 外部データ形式       | CSV                                |

<p align="right">(<a href="#top">トップへ</a>)</p>

## ディレクトリ構成
```
.
├── Assets/
│   ├── Fonts/               # フォントアセット（LFS管理対象）
│   ├── Resources/           # 動的に読み込むCSVデータ等のリソース
│   ├── Scenes/              # ゲームの各シーンファイル
│   └── Scripts/             # C#スクリプト
│       ├── GameSystem/      # タイピング判定・ロジック基盤
│       ├── UI/              # 演出・UI制御
│       └── DataHandler/     # CSV読み込み・データ処理
├── Packages/                # Unityパッケージ管理
├── ProjectSettings/         # Unityプロジェクト設定
├── .gitattributes           # Git LFSの設定
├── .gitignore               # Unity用除外設定
└── README.md                # 本ファイル
```

<p align="right">(<a href="#top">トップへ</a>)</p>

## 主な機能

### ゲームシステム
* **逆転学習ルール**: ローマ字入力の「子音+母音」を「母音+子音」へと反転させる特殊ルールを実装しました．
* **難易度選択**: 初学者から上級者まで対応する3段階の難易度設定を搭載しています．
* **動的パラメータ調整**: 連続正解によるタイムボーナスなど，認知的負荷と爽快感のバランスを保つ仕組みを構築しました．

### データ管理
* **CSV外部連携**: 問題データを外部CSVから動的に読み込むことで，コードを修正せずに問題の追加・変更が可能な設計にしました．

<p align="right">(<a href="#top">トップへ</a>)</p>

## 設計における工夫と考察

### 1. データとロジックの分離
ハッカソンという短期間のチーム開発において，エンジニア以外のメンバーも問題編集に関与できるよう，CSVによるデータ駆動型の設計を採用しました．これにより，実装とコンテンツ制作の並行作業を効率化しました．

### 2. 認知的負荷の制御
単に難しくするのではなく，「無意識の破壊」というコンセプトに基づき，適切な視覚フィードバックとサウンド演出を組み合わせることで，高負荷なルール下でもUXを損なわない設計を検討・実施しました．

<p align="right">(<a href="#top">トップへ</a>)</p>

## 開発者情報
* **Name**: Takato Ishii
* **Portfolio**: [https://takato-ishii.vercel.app/](https://takato-ishii.vercel.app/)

<p align="right">(<a href="#top">トップへ</a>)</p>