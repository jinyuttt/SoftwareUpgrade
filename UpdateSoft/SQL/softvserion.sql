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

 Date: 15/05/2019 02:27:47
*/


-- ----------------------------
-- Table structure for softvserion
-- ----------------------------
DROP TABLE IF EXISTS "public"."softvserion";
CREATE TABLE "public"."softvserion" (
  "id" int4 NOT NULL,
  "version" varchar(255) COLLATE "pg_catalog"."default",
  "vertime" varchar(255) COLLATE "pg_catalog"."default",
  "versionid" int4,
  "clear" bool NOT NULL DEFAULT false,
  "DataTime" timestamp(0) DEFAULT now()
)
;
COMMENT ON COLUMN "public"."softvserion"."id" IS '主键';
COMMENT ON COLUMN "public"."softvserion"."version" IS '版本号';
COMMENT ON COLUMN "public"."softvserion"."vertime" IS '版本时间';
COMMENT ON COLUMN "public"."softvserion"."versionid" IS '版本控制号';
COMMENT ON COLUMN "public"."softvserion"."clear" IS '该版本是否清库';
COMMENT ON COLUMN "public"."softvserion"."DataTime" IS '该行数据时间';

-- ----------------------------
-- Primary Key structure for table softvserion
-- ----------------------------
ALTER TABLE "public"."softvserion" ADD CONSTRAINT "softvserion_pkey" PRIMARY KEY ("id");
