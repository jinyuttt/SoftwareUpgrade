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

 Date: 15/05/2019 02:27:03
*/


-- ----------------------------
-- Table structure for updatesoft
-- ----------------------------
DROP TABLE IF EXISTS "public"."updatesoft";
CREATE TABLE "public"."updatesoft" (
  "id" int4 NOT NULL DEFAULT '-1'::integer,
  "name" varchar(255) COLLATE "pg_catalog"."default",
  "md5" varchar(255) COLLATE "pg_catalog"."default",
  "parentid" int4,
  "content" bytea,
  "version" varchar(255) COLLATE "pg_catalog"."default",
  "isdirectory" bool,
  "nodeid" int4,
  "DataTime" timestamp(0) DEFAULT now(),
  "storeid" int4 DEFAULT '-1'::integer,
  "path" varchar(255) COLLATE "pg_catalog"."default"
)
;
COMMENT ON COLUMN "public"."updatesoft"."id" IS '主键';
COMMENT ON COLUMN "public"."updatesoft"."name" IS '名称';
COMMENT ON COLUMN "public"."updatesoft"."md5" IS 'MD5值';
COMMENT ON COLUMN "public"."updatesoft"."parentid" IS '父节点';
COMMENT ON COLUMN "public"."updatesoft"."content" IS '文件内容';
COMMENT ON COLUMN "public"."updatesoft"."version" IS '版本号';
COMMENT ON COLUMN "public"."updatesoft"."isdirectory" IS '是否是目录节点';
COMMENT ON COLUMN "public"."updatesoft"."nodeid" IS '节点ID';
COMMENT ON COLUMN "public"."updatesoft"."DataTime" IS '改行数据时间';
COMMENT ON COLUMN "public"."updatesoft"."storeid" IS 'lagrestore的主键，但是不建立外键';
COMMENT ON COLUMN "public"."updatesoft"."path" IS '路径备用（当前同Name）';

-- ----------------------------
-- Primary Key structure for table updatesoft
-- ----------------------------
ALTER TABLE "public"."updatesoft" ADD CONSTRAINT "updatesoft_pkey" PRIMARY KEY ("id");
