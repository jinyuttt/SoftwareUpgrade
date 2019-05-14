/*
 Navicat Premium Data Transfer

 Source Server         : local
 Source Server Type    : PostgreSQL
 Source Server Version : 110002
 Source Host           : localhost:5432
 Source Catalog        : postgres
 Source Schema         : public

 Target Server Type    : PostgreSQL
 Target Server Version : 110002
 File Encoding         : 65001

 Date: 15/05/2019 02:27:54
*/


-- ----------------------------
-- Table structure for largestore
-- ----------------------------
DROP TABLE IF EXISTS "public"."largestore";
CREATE TABLE "public"."largestore" (
  "id" int4 NOT NULL,
  "path" varchar(255) COLLATE "pg_catalog"."default",
  "md5" varchar(255) COLLATE "pg_catalog"."default",
  "name" varchar(255) COLLATE "pg_catalog"."default",
  "localname" varchar(255) COLLATE "pg_catalog"."default",
  "DataTime" timestamp(6) DEFAULT now()
)
;
COMMENT ON COLUMN "public"."largestore"."id" IS '主键';
COMMENT ON COLUMN "public"."largestore"."path" IS '路径';
COMMENT ON COLUMN "public"."largestore"."md5" IS '文件MD5';
COMMENT ON COLUMN "public"."largestore"."name" IS '文件名称';
COMMENT ON COLUMN "public"."largestore"."localname" IS '本地名称，重名使用';
COMMENT ON COLUMN "public"."largestore"."DataTime" IS '此行数据时间';

-- ----------------------------
-- Primary Key structure for table largestore
-- ----------------------------
ALTER TABLE "public"."largestore" ADD CONSTRAINT "largestore_pkey" PRIMARY KEY ("id");
