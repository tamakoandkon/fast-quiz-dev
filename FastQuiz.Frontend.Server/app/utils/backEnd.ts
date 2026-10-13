/* eslint-disable */
/* tslint:disable */
// @ts-nocheck
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

export enum QuizAttemptStatus {
  Pending = "Pending",
  InProgress = "InProgress",
  Submitted = "Submitted",
  Graded = "Graded",
}

export enum QuestionType {
  SingleChoice = "SingleChoice",
  MultipleChoice = "MultipleChoice",
  FillIn = "FillIn",
}

export enum MatchStrategy {
  Exact = "Exact",
  Contains = "Contains",
  Regex = "Regex",
  Manual = "Manual",
}

export interface AnswerRecordDto {
  id?: string | null;
  quizAttemptId?: string | null;
  questionRecordId?: string | null;
  value?: string | null;
}

export interface AuthDto {
  accessToken?: string | null;
  /** @format int64 */
  expiresIn?: number;
  user?: UserDto;
}

export interface CreateOrganizationCommand {
  name?: string | null;
  parentId?: string | null;
}

export interface CreateQuestionAnswerCommand {
  content?: string | null;
  isCorrect?: boolean;
  /** @format int32 */
  sortOrder?: number;
  matchStrategy?: MatchStrategy;
}

export interface CreateQuestionCommand {
  content?: string | null;
  /** @format int32 */
  level?: number;
  type?: QuestionType;
  answers?: CreateQuestionAnswerCommand[] | null;
  parentId?: string | null;
  remark?: string | null;
}

export interface CreateQuestionGroupCommand {
  name?: string | null;
  remark?: string | null;
}

export interface CreateQuestionSelectionRuleCommand {
  groupId?: string | null;
  questionType?: QuestionType;
  /** @format int32 */
  level?: number;
  /** @format int32 */
  count?: number;
  /** @format int32 */
  score?: number;
}

export interface CreateQuizAttemptCommand {
  quizSnapshotId?: string | null;
  /** @format int32 */
  count?: number;
}

export interface CreateQuizSnapshotCommand {
  /** @format int32 */
  count?: number;
}

export interface CreateQuizTemplateCommand {
  name?: string | null;
  description?: string | null;
  remark?: string | null;
  /** @format int32 */
  attemptLimit?: number;
  isUnlimitedTime?: boolean;
  /** @format date-time */
  beginTime?: string;
  /** @format date-time */
  endTime?: string;
  /** @format int32 */
  durationMinutes?: number;
  userGroupId?: string | null;
}

export interface CreateRoleCommand {
  name?: string | null;
  description?: string | null;
  isSystem?: boolean;
  isEnable?: boolean;
  permissions?: string[] | null;
}

export interface CreateTenantCommand {
  name?: string | null;
  description?: string | null;
  adminUsername?: string | null;
  adminPassword?: string | null;
  adminEmail?: string | null;
}

export interface DeleteQuestionGroupCommand {
  groupIds?: string[] | null;
}

export interface OrganizationDto {
  id?: string | null;
  name?: string | null;
  tenantId?: string | null;
  parentId?: string | null;
  /** @format date-time */
  createdAt?: string;
  /** @format date-time */
  updatedAt?: string;
}

export interface ProblemDetails {
  type?: string | null;
  title?: string | null;
  /** @format int32 */
  status?: number | null;
  detail?: string | null;
  instance?: string | null;
  [key: string]: any;
}

export interface QuestionAnswerDto {
  id?: string | null;
  questionId?: string | null;
  /** @format int32 */
  sortOrder?: number;
  markupContent?: string | null;
  isCorrect?: boolean;
  matchStrategy?: MatchStrategy;
}

export interface QuestionAnswerRecordWithoutAnswerDto {
  id?: string | null;
  markUpContent?: string | null;
  questionSnapshotId?: string | null;
  /** @format int32 */
  sortOrder?: number;
  matchStrategy?: MatchStrategy;
}

export interface QuestionAnswerSnapshotDto {
  id?: string | null;
  markUpContent?: string | null;
  isCorrect?: boolean;
  questionSnapshotId?: string | null;
  /** @format int32 */
  sortOrder?: number;
  matchStrategy?: MatchStrategy;
}

export interface QuestionDto {
  id?: string | null;
  type?: QuestionType;
  /** @format int32 */
  level?: number;
  remark?: string | null;
  groupId?: string | null;
  markupContent?: string | null;
  answers?: QuestionAnswerDto[] | null;
}

export interface QuestionGroupDto {
  id?: string | null;
  ownerId?: string | null;
  name?: string | null;
  remark?: string | null;
  /** @format int32 */
  count?: number;
}

export interface QuestionGroupStatisticsDto {
  /** @format int32 */
  totalCount?: number;
  types?: QuestionTypeCountDto[] | null;
  levels?: QuestionLevelCountDto[] | null;
}

export interface QuestionLevelCountDto {
  /** @format int32 */
  level?: number;
  /** @format int32 */
  count?: number;
}

export interface QuestionRecordWithAnswerDto {
  id?: string | null;
  quizAttemptId?: string | null;
  questionSnapshotId?: string | null;
  markUpContent?: string | null;
  type?: QuestionType;
  /** @format int32 */
  level?: number;
  groupId?: string | null;
  parentId?: string | null;
  answers?: QuestionAnswerSnapshotDto[] | null;
  answerRecords?: AnswerRecordDto[] | null;
  isCorrect?: boolean;
}

export interface QuestionRecordWithoutAnswerDto {
  id?: string | null;
  quizAttemptId?: string | null;
  questionSnapshotId?: string | null;
  markUpContent?: string | null;
  type?: QuestionType;
  /** @format int32 */
  level?: number;
  groupId?: string | null;
  parentId?: string | null;
  answers?: QuestionAnswerRecordWithoutAnswerDto[] | null;
  answerRecords?: AnswerRecordDto[] | null;
}

export interface QuestionSelectionRuleDto {
  id?: string | null;
  quizTemplateId?: string | null;
  groupId?: string | null;
  questionType?: QuestionType;
  /** @format int32 */
  level?: number;
  /** @format int32 */
  count?: number;
  /** @format int32 */
  score?: number;
}

export interface QuestionSnapshotDto {
  id?: string | null;
  quizTemplateId?: string | null;
  questionId?: string | null;
  markUpContent?: string | null;
  type?: QuestionType;
  /** @format int32 */
  level?: number;
  groupId?: string | null;
  parentId?: string | null;
  answers?: QuestionAnswerSnapshotDto[] | null;
}

export interface QuestionTypeCountDto {
  type?: QuestionType;
  /** @format int32 */
  count?: number;
}

export interface QuizAttempDto {
  id?: string | null;
  assignId?: string | null;
  quizTemplateId?: string | null;
  quizTemplateNema?: string | null;
  quizSampleId?: string | null;
  /** @format date-time */
  startTime?: string | null;
  /** @format date-time */
  submitTime?: string | null;
  /** @format int32 */
  score?: number;
  status?: QuizAttemptStatus;
}

export interface QuizSnapshotDto {
  id?: string | null;
  quizTemplateId?: string | null;
  /** @format int32 */
  totalScore?: number;
}

export interface QuizTemplateDto {
  id?: string | null;
  name?: string | null;
  description?: string | null;
  remark?: string | null;
  /** @format int32 */
  attemptLimit?: number;
  isUnlimitedTime?: boolean;
  /** @format date-time */
  beginTime?: string;
  /** @format date-time */
  endTime?: string;
  /** @format int32 */
  durationMinutes?: number;
  isPublished?: boolean;
  ownerId?: string | null;
  /** @format date-time */
  createdAt?: string;
  /** @format date-time */
  updatedAt?: string;
  userGroupId?: string | null;
}

export interface RoleDto {
  id?: string | null;
  name?: string | null;
  description?: string | null;
  tenantId?: string | null;
  isSystem?: boolean;
  isEnable?: boolean;
  /** @format date-time */
  createdAt?: string;
  /** @format date-time */
  updatedAt?: string;
  permissions?: string[] | null;
}

export interface SetAnswerRecordCommand {
  value?: string | null;
}

export interface TenantDto {
  id?: string | null;
  name?: string | null;
  description?: string | null;
  /** @format date-time */
  createdAt?: string;
  /** @format date-time */
  updatedAt?: string;
}

export interface UpdateOrganizationCommand {
  name?: string | null;
}

export interface UpdateQuestionCommand {
  content?: string | null;
  /** @format int32 */
  level?: number;
  type?: QuestionType;
  answers?: CreateQuestionAnswerCommand[] | null;
  remark?: string | null;
}

export interface UpdateQuestionGroupCommand {
  name?: string | null;
  remark?: string | null;
}

export interface UpdateQuestionSelectionRuleCommand {
  groupId?: string | null;
  questionType?: QuestionType;
  /** @format int32 */
  level?: number;
  /** @format int32 */
  count?: number;
  /** @format int32 */
  score?: number;
}

export interface UpdateQuizTemplateCommand {
  name?: string | null;
  description?: string | null;
  remark?: string | null;
  /** @format int32 */
  attemptLimit?: number;
  isUnlimitedTime?: boolean;
  /** @format date-time */
  beginTime?: string;
  /** @format date-time */
  endTime?: string;
  /** @format int32 */
  durationMinutes?: number;
  userGroupId?: string | null;
}

export interface UpdateQuizTemplatePublishedCommand {
  isPublished?: boolean;
}

export interface UpdateRoleCommand {
  name?: string | null;
  description?: string | null;
  isSystem?: boolean;
  isEnable?: boolean;
  permissions?: string[] | null;
}

export interface UpdateTenantCommand {
  name?: string | null;
  description?: string | null;
}

export interface UpdateUserCommand {
  username?: string | null;
  password?: string | null;
  email?: string | null;
}

export interface UserDto {
  id?: string | null;
  username?: string | null;
  email?: string | null;
  /** @format date-time */
  createdAt?: string | null;
}

export interface UserLoginCommand {
  username?: string | null;
  password?: string | null;
}

export interface UserRegisterCommand {
  username?: string | null;
  password?: string | null;
  email?: string | null;
}

export namespace Auth {
  /**
   * No description
   * @tags Auth
   * @name RegisterCreate
   * @request POST:/auth/register
   */
  export namespace RegisterCreate {
    export type RequestParams = {};
    export type RequestQuery = {};
    export type RequestBody = UserRegisterCommand;
    export type RequestHeaders = {};
    export type ResponseBody = AuthDto;
  }

  /**
   * No description
   * @tags Auth
   * @name LoginCreate
   * @request POST:/auth/login
   */
  export namespace LoginCreate {
    export type RequestParams = {};
    export type RequestQuery = {};
    export type RequestBody = UserLoginCommand;
    export type RequestHeaders = {};
    export type ResponseBody = AuthDto;
  }

  /**
   * No description
   * @tags Auth
   * @name ProfileList
   * @request GET:/auth/profile
   */
  export namespace ProfileList {
    export type RequestParams = {};
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = UserDto;
  }

  /**
   * No description
   * @tags Auth
   * @name PutAuth
   * @request PUT:/auth/me
   */
  export namespace PutAuth {
    export type RequestParams = {};
    export type RequestQuery = {};
    export type RequestBody = UpdateUserCommand;
    export type RequestHeaders = {};
    export type ResponseBody = UserDto;
  }
}

export namespace Tenant {
  /**
   * No description
   * @tags Organization
   * @name OrganizationCreate
   * @request POST:/tenant/{tenantId}/organization
   */
  export namespace OrganizationCreate {
    export type RequestParams = {
      tenantId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = CreateOrganizationCommand;
    export type RequestHeaders = {};
    export type ResponseBody = OrganizationDto;
  }

  /**
   * No description
   * @tags Organization
   * @name OrganizationList
   * @request GET:/tenant/{tenantId}/organization
   */
  export namespace OrganizationList {
    export type RequestParams = {
      tenantId: string;
    };
    export type RequestQuery = {
      Cursor?: string;
      /** @format int32 */
      Count: number;
      IsPrevious: boolean;
    };
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = OrganizationDto[];
  }

  /**
   * No description
   * @tags Organization
   * @name OrganizationDetail
   * @request GET:/tenant/{tenantId}/organization/{parentId}
   */
  export namespace OrganizationDetail {
    export type RequestParams = {
      tenantId: string;
      parentId: string;
    };
    export type RequestQuery = {
      Cursor?: string;
      /** @format int32 */
      Count: number;
      IsPrevious: boolean;
    };
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = OrganizationDto[];
  }

  /**
   * No description
   * @tags Organization
   * @name OrganizationDetail2
   * @request GET:/tenant/organization/{orgId}
   * @originalName organizationDetail
   * @duplicate
   */
  export namespace OrganizationDetail2 {
    export type RequestParams = {
      orgId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = OrganizationDto;
  }

  /**
   * No description
   * @tags Organization
   * @name OrganizationUpdate
   * @request PUT:/tenant/organization/{orgId}
   */
  export namespace OrganizationUpdate {
    export type RequestParams = {
      orgId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = UpdateOrganizationCommand;
    export type RequestHeaders = {};
    export type ResponseBody = void;
  }

  /**
   * No description
   * @tags Organization
   * @name OrganizationDelete
   * @request DELETE:/tenant/organization/{orgId}
   */
  export namespace OrganizationDelete {
    export type RequestParams = {
      orgId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = void;
  }

  /**
   * No description
   * @tags Organization
   * @name OrganizationUserCreate
   * @request POST:/tenant/organization/{orgId}/user/{userId}
   */
  export namespace OrganizationUserCreate {
    export type RequestParams = {
      orgId: string;
      userId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = OrganizationDto;
  }

  /**
   * No description
   * @tags Organization
   * @name OrganizationUserDelete
   * @request DELETE:/tenant/organization{orgId}/user/{userId}
   */
  export namespace OrganizationUserDelete {
    export type RequestParams = {
      orgId: string;
      userId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = void;
  }

  /**
   * No description
   * @tags QuestionGroup
   * @name QuestionGroupCreate
   * @request POST:/tenant/{tenantId}/question-group
   */
  export namespace QuestionGroupCreate {
    export type RequestParams = {
      tenantId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = CreateQuestionGroupCommand;
    export type RequestHeaders = {};
    export type ResponseBody = QuestionGroupDto;
  }

  /**
   * No description
   * @tags QuestionGroup
   * @name QuestionGroupList
   * @request GET:/tenant/{tenantId}/question-group
   */
  export namespace QuestionGroupList {
    export type RequestParams = {
      tenantId: string;
    };
    export type RequestQuery = {
      Cursor?: string;
      /** @format int32 */
      Count?: number;
      IsPrevious?: boolean;
      NamePrefix?: string;
    };
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = QuestionGroupDto[];
  }

  /**
   * No description
   * @tags QuestionGroup
   * @name QuestionGroupBatchDeleteCreate
   * @request POST:/tenant/{tenantId}/question-group/batch-delete
   */
  export namespace QuestionGroupBatchDeleteCreate {
    export type RequestParams = {
      tenantId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = DeleteQuestionGroupCommand;
    export type RequestHeaders = {};
    export type ResponseBody = void;
  }

  /**
   * No description
   * @tags QuestionGroup
   * @name QuestionGroupDetail
   * @request GET:/tenant/{tenantId}/question-group/{groupId}
   */
  export namespace QuestionGroupDetail {
    export type RequestParams = {
      tenantId: string;
      groupId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = QuestionGroupDto;
  }

  /**
   * No description
   * @tags QuestionGroup
   * @name QuestionGroupUpdate
   * @request PUT:/tenant/{tenantId}/question-group/{groupId}
   */
  export namespace QuestionGroupUpdate {
    export type RequestParams = {
      tenantId: string;
      groupId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = UpdateQuestionGroupCommand;
    export type RequestHeaders = {};
    export type ResponseBody = void;
  }

  /**
   * No description
   * @tags QuestionGroup
   * @name QuestionGroupDelete
   * @request DELETE:/tenant/{tenantId}/question-group/{groupId}
   */
  export namespace QuestionGroupDelete {
    export type RequestParams = {
      tenantId: string;
      groupId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = void;
  }

  /**
   * No description
   * @tags QuestionGroup
   * @name QuestionGroupCountList
   * @request GET:/tenant/{tenantId}/question-group/{groupId}/count
   */
  export namespace QuestionGroupCountList {
    export type RequestParams = {
      tenantId: string;
      groupId: string;
    };
    export type RequestQuery = {
      Type: QuestionType;
      /** @format int32 */
      Level: number;
    };
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = number;
  }

  /**
   * No description
   * @tags QuestionGroup
   * @name QuestionGroupStatisticsList
   * @request GET:/tenant/{tenantId}/question-group/{groupId}/statistics
   */
  export namespace QuestionGroupStatisticsList {
    export type RequestParams = {
      tenantId: string;
      groupId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = QuestionGroupStatisticsDto;
  }

  /**
   * No description
   * @tags QuestionGroup
   * @name QuestionGroupQuestionCreate
   * @request POST:/tenant/{tenantId}/question-group/{groupId}/question
   */
  export namespace QuestionGroupQuestionCreate {
    export type RequestParams = {
      tenantId: string;
      groupId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = CreateQuestionCommand;
    export type RequestHeaders = {};
    export type ResponseBody = QuestionDto;
  }

  /**
   * No description
   * @tags QuestionGroup
   * @name QuestionGroupQuestionDetail
   * @request GET:/tenant/{tenantId}/question-group/question/{questionId}
   */
  export namespace QuestionGroupQuestionDetail {
    export type RequestParams = {
      tenantId: string;
      questionId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = QuestionDto;
  }

  /**
   * No description
   * @tags QuestionGroup
   * @name QuestionGroupQuestionUpdate
   * @request PUT:/tenant/{tenantId}/question-group/question/{questionId}
   */
  export namespace QuestionGroupQuestionUpdate {
    export type RequestParams = {
      tenantId: string;
      questionId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = UpdateQuestionCommand;
    export type RequestHeaders = {};
    export type ResponseBody = void;
  }

  /**
   * No description
   * @tags QuestionGroup
   * @name QuestionGroupQuestionDelete
   * @request DELETE:/tenant/{tenantId}/question-group/question/{questionId}
   */
  export namespace QuestionGroupQuestionDelete {
    export type RequestParams = {
      tenantId: string;
      questionId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = void;
  }

  /**
   * No description
   * @tags QuestionGroup
   * @name QuestionGroupQuestionList
   * @request GET:/tenant/{tenantId}/question-group/{questionGroupId}/question
   */
  export namespace QuestionGroupQuestionList {
    export type RequestParams = {
      tenantId: string;
      questionGroupId: string;
    };
    export type RequestQuery = {
      Cursor?: string;
      Type?: QuestionType;
      /** @format int32 */
      Level?: number;
      /** @format int32 */
      Count?: number;
      IsPrevious?: boolean;
    };
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = QuestionDto[];
  }

  /**
   * No description
   * @tags QuizAttempt
   * @name QuizTemplateQuizAttemptCreate
   * @request POST:/tenant/{tenantId}/quiz-template/{quizTemplateId}/quiz-attempt
   */
  export namespace QuizTemplateQuizAttemptCreate {
    export type RequestParams = {
      tenantId: string;
      quizTemplateId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = QuizAttempDto;
  }

  /**
   * No description
   * @tags QuizAttempt
   * @name QuizTemplateQuizAttemptList
   * @request GET:/tenant/{tenantId}/quiz-template/{quizTemplateId}/quiz-attempt
   */
  export namespace QuizTemplateQuizAttemptList {
    export type RequestParams = {
      tenantId: string;
      quizTemplateId: string;
    };
    export type RequestQuery = {
      Cursor?: string;
      /** @format int32 */
      Count?: number;
      IsPrevious?: boolean;
    };
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = QuizAttempDto[];
  }

  /**
   * No description
   * @tags QuizAttempt
   * @name QuizTemplateQuizAttemptBatchCreate
   * @request POST:/tenant/{tenantId}/quiz-template/{quizTemplateId}/quiz-attempt/batch
   */
  export namespace QuizTemplateQuizAttemptBatchCreate {
    export type RequestParams = {
      tenantId: string;
      quizTemplateId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = CreateQuizAttemptCommand[];
    export type RequestHeaders = {};
    export type ResponseBody = void;
  }

  /**
   * No description
   * @tags QuizAttempt
   * @name QuizTemplateQuizAttemptRecordsList
   * @request GET:/tenant/{tenantId}/quiz-template/{quizTemplateId}/quiz-attempt/records
   */
  export namespace QuizTemplateQuizAttemptRecordsList {
    export type RequestParams = {
      tenantId: string;
      quizTemplateId: string;
    };
    export type RequestQuery = {
      OnlyBest?: boolean;
      Cursor?: string;
      /** @format int32 */
      Count?: number;
      IsPrevious?: boolean;
    };
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = QuizAttempDto[];
  }

  /**
   * No description
   * @tags QuizAttempt
   * @name QuizTemplateQuizAttemptMeList
   * @request GET:/tenant/{tenantId}/quiz-template/quiz-attempt/me
   */
  export namespace QuizTemplateQuizAttemptMeList {
    export type RequestParams = {
      tenantId: string;
    };
    export type RequestQuery = {
      Cursor?: string;
      /** @format int32 */
      Count?: number;
    };
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = QuizAttempDto[];
  }

  /**
   * No description
   * @tags QuizAttempt
   * @name QuizTemplateQuizAttemptMeTemplateDetail
   * @request GET:/tenant/{tenantId}/quiz-template/quiz-attempt/me/template/{quizTemplateId}
   */
  export namespace QuizTemplateQuizAttemptMeTemplateDetail {
    export type RequestParams = {
      tenantId: string;
      quizTemplateId: string;
    };
    export type RequestQuery = {
      Cursor?: string;
      /** @format int32 */
      Count?: number;
      IsPrevious?: boolean;
    };
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = QuizAttempDto[];
  }

  /**
   * No description
   * @tags QuizAttempt
   * @name QuizTemplateQuizAttemptUserRecordsList
   * @request GET:/tenant/{tenantId}/quiz-template/quiz-attempt/user/{userId}/records
   */
  export namespace QuizTemplateQuizAttemptUserRecordsList {
    export type RequestParams = {
      tenantId: string;
      userId: string;
    };
    export type RequestQuery = {
      TemplateId?: string;
      OnlyBest?: boolean;
      Cursor?: string;
      /** @format int32 */
      Count?: number;
      IsPrevious?: boolean;
    };
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = QuizAttempDto[];
  }

  /**
   * No description
   * @tags QuizAttempt
   * @name QuizTemplateQuizAttemptDetail
   * @request GET:/tenant/{tenantId}/quiz-template/quiz-attempt/{quizAttemptId}
   */
  export namespace QuizTemplateQuizAttemptDetail {
    export type RequestParams = {
      tenantId: string;
      quizAttemptId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = QuizAttempDto;
  }

  /**
   * No description
   * @tags QuizAttempt
   * @name QuizTemplateQuizAttemptUpdate
   * @request PUT:/tenant/{tenantId}/quiz-template/quiz-attempt/{quizAttemptId}
   */
  export namespace QuizTemplateQuizAttemptUpdate {
    export type RequestParams = {
      tenantId: string;
      quizAttemptId: string;
    };
    export type RequestQuery = {
      status: QuizAttemptStatus;
    };
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = void;
  }

  /**
   * No description
   * @tags QuizAttempt
   * @name QuizTemplateQuizAttemptQuestionList
   * @request GET:/tenant/{tenantId}/quiz-template/quiz-attempt/{quizAttemptId}/question
   */
  export namespace QuizTemplateQuizAttemptQuestionList {
    export type RequestParams = {
      tenantId: string;
      quizAttemptId: string;
    };
    export type RequestQuery = {
      Cursor?: string;
      /** @format int32 */
      Count?: number;
    };
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = QuestionRecordWithoutAnswerDto[];
  }

  /**
   * No description
   * @tags QuizAttempt
   * @name QuizTemplateQuizAttemptQuestionWithAnswerList
   * @request GET:/tenant/{tenantId}/quiz-template/quiz-attempt/{quizAttemptId}/question/with-answer
   */
  export namespace QuizTemplateQuizAttemptQuestionWithAnswerList {
    export type RequestParams = {
      tenantId: string;
      quizAttemptId: string;
    };
    export type RequestQuery = {
      Cursor?: string;
      /** @format int32 */
      Count?: number;
    };
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = QuestionRecordWithAnswerDto[];
  }

  /**
   * No description
   * @tags QuizAttempt
   * @name QuizTemplateQuizAttemptQuestionUpdate
   * @request PUT:/tenant/{tenantId}/quiz-template/quiz-attempt/question/{questionId}
   */
  export namespace QuizTemplateQuizAttemptQuestionUpdate {
    export type RequestParams = {
      tenantId: string;
      questionId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = SetAnswerRecordCommand[];
    export type RequestHeaders = {};
    export type ResponseBody = void;
  }

  /**
   * No description
   * @tags QuizAttempt
   * @name QuizTemplateQuizAttemptQuestionStatusUpdate
   * @request PUT:/tenant/{tenantId}/quiz-template/quiz-attempt/question/{questionId}/status/{isCorrect}
   */
  export namespace QuizTemplateQuizAttemptQuestionStatusUpdate {
    export type RequestParams = {
      tenantId: string;
      questionId: string;
      isCorrect: boolean;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = void;
  }

  /**
   * No description
   * @tags QuizSnapshot
   * @name QuizTemplateQuizSnapshotCreate
   * @request POST:/tenant/{tenantId}/quiz-template/{quizTemplateId}/quiz-snapshot
   */
  export namespace QuizTemplateQuizSnapshotCreate {
    export type RequestParams = {
      tenantId: string;
      quizTemplateId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = CreateQuizSnapshotCommand;
    export type RequestHeaders = {};
    export type ResponseBody = void;
  }

  /**
   * No description
   * @tags QuizSnapshot
   * @name QuizTemplateQuizSnapshotList
   * @request GET:/tenant/{tenantId}/quiz-template/{quizTemplateId}/quiz-snapshot
   */
  export namespace QuizTemplateQuizSnapshotList {
    export type RequestParams = {
      tenantId: string;
      quizTemplateId: string;
    };
    export type RequestQuery = {
      Cursor?: string;
      /** @format int32 */
      Count?: number;
    };
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = QuizSnapshotDto[];
  }

  /**
   * No description
   * @tags QuizSnapshot
   * @name QuizTemplateQuizSnapshotDetail
   * @request GET:/tenant/{tenantId}/quiz-template/quiz-snapshot/{quizSnapshotId}
   */
  export namespace QuizTemplateQuizSnapshotDetail {
    export type RequestParams = {
      tenantId: string;
      quizSnapshotId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = QuizSnapshotDto;
  }

  /**
   * No description
   * @tags QuizSnapshot
   * @name QuizTemplateQuizSnapshotDelete
   * @request DELETE:/tenant/{tenantId}/quiz-template/quiz-snapshot/{quizSnapshotId}
   */
  export namespace QuizTemplateQuizSnapshotDelete {
    export type RequestParams = {
      tenantId: string;
      quizSnapshotId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = void;
  }

  /**
   * No description
   * @tags QuizSnapshot
   * @name QuizTemplateQuizSnapshotQuestionSnapshotList
   * @request GET:/tenant/{tenantId}/quiz-template/quiz-snapshot/{quizSnapshotId}/question-snapshot
   */
  export namespace QuizTemplateQuizSnapshotQuestionSnapshotList {
    export type RequestParams = {
      tenantId: string;
      quizSnapshotId: string;
    };
    export type RequestQuery = {
      Cursor?: string;
      /** @format int32 */
      Count?: number;
    };
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = QuestionSnapshotDto[];
  }

  /**
   * No description
   * @tags QuizTemplate
   * @name QuizTemplateCreate
   * @request POST:/tenant/{tenantId}/quiz-template
   */
  export namespace QuizTemplateCreate {
    export type RequestParams = {
      tenantId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = CreateQuizTemplateCommand;
    export type RequestHeaders = {};
    export type ResponseBody = QuizTemplateDto;
  }

  /**
   * No description
   * @tags QuizTemplate
   * @name QuizTemplateList
   * @request GET:/tenant/{tenantId}/quiz-template
   */
  export namespace QuizTemplateList {
    export type RequestParams = {
      tenantId: string;
    };
    export type RequestQuery = {
      Cursor?: string;
      /** @format int32 */
      Count?: number;
      IsPrevious?: boolean;
    };
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = QuizTemplateDto[];
  }

  /**
   * No description
   * @tags QuizTemplate
   * @name QuizTemplatePublishedList
   * @request GET:/tenant/{tenantId}/quiz-template/published
   */
  export namespace QuizTemplatePublishedList {
    export type RequestParams = {
      tenantId: string;
    };
    export type RequestQuery = {
      Cursor?: string;
      /** @format int32 */
      Count?: number;
    };
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = QuizTemplateDto[];
  }

  /**
   * No description
   * @tags QuizTemplate
   * @name QuizTemplateDetail
   * @request GET:/tenant/{tenantId}/quiz-template/{quizTemplateId}
   */
  export namespace QuizTemplateDetail {
    export type RequestParams = {
      tenantId: string;
      quizTemplateId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = QuizTemplateDto;
  }

  /**
   * No description
   * @tags QuizTemplate
   * @name QuizTemplateUpdate
   * @request PUT:/tenant/{tenantId}/quiz-template/{quizTemplateId}
   */
  export namespace QuizTemplateUpdate {
    export type RequestParams = {
      tenantId: string;
      quizTemplateId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = UpdateQuizTemplateCommand;
    export type RequestHeaders = {};
    export type ResponseBody = void;
  }

  /**
   * No description
   * @tags QuizTemplate
   * @name QuizTemplateDelete
   * @request DELETE:/tenant/{tenantId}/quiz-template/{quizTemplateId}
   */
  export namespace QuizTemplateDelete {
    export type RequestParams = {
      tenantId: string;
      quizTemplateId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = void;
  }

  /**
   * No description
   * @tags QuizTemplate
   * @name QuizTemplatePublishedUpdate
   * @request PUT:/tenant/{tenantId}/quiz-template/{quizTemplateId}/published
   */
  export namespace QuizTemplatePublishedUpdate {
    export type RequestParams = {
      tenantId: string;
      quizTemplateId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = UpdateQuizTemplatePublishedCommand;
    export type RequestHeaders = {};
    export type ResponseBody = void;
  }

  /**
   * No description
   * @tags QuizTemplate
   * @name QuizTemplateRuleCreate
   * @request POST:/tenant/{tenantId}/quiz-template/{quizTemplateId}/rule
   */
  export namespace QuizTemplateRuleCreate {
    export type RequestParams = {
      tenantId: string;
      quizTemplateId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = CreateQuestionSelectionRuleCommand;
    export type RequestHeaders = {};
    export type ResponseBody = QuestionSelectionRuleDto;
  }

  /**
   * No description
   * @tags QuizTemplate
   * @name QuizTemplateRuleList
   * @request GET:/tenant/{tenantId}/quiz-template/{quizTemplateId}/rule
   */
  export namespace QuizTemplateRuleList {
    export type RequestParams = {
      tenantId: string;
      quizTemplateId: string;
    };
    export type RequestQuery = {
      Cursor?: string;
      /** @format int32 */
      Count?: number;
    };
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = QuestionSelectionRuleDto[];
  }

  /**
   * No description
   * @tags QuizTemplate
   * @name QuizTemplateRuleDetail
   * @request GET:/tenant/{tenantId}/quiz-template/rule/{ruleId}
   */
  export namespace QuizTemplateRuleDetail {
    export type RequestParams = {
      tenantId: string;
      ruleId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = QuestionSelectionRuleDto;
  }

  /**
   * No description
   * @tags QuizTemplate
   * @name QuizTemplateRuleUpdate
   * @request PUT:/tenant/{tenantId}/quiz-template/rule/{ruleId}
   */
  export namespace QuizTemplateRuleUpdate {
    export type RequestParams = {
      tenantId: string;
      ruleId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = UpdateQuestionSelectionRuleCommand;
    export type RequestHeaders = {};
    export type ResponseBody = void;
  }

  /**
   * No description
   * @tags QuizTemplate
   * @name QuizTemplateRuleDelete
   * @request DELETE:/tenant/{tenantId}/quiz-template/rule/{ruleId}
   */
  export namespace QuizTemplateRuleDelete {
    export type RequestParams = {
      tenantId: string;
      ruleId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = void;
  }

  /**
   * No description
   * @tags Role
   * @name RoleCreate
   * @request POST:/tenant/{tenantId}/role
   */
  export namespace RoleCreate {
    export type RequestParams = {
      tenantId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = CreateRoleCommand;
    export type RequestHeaders = {};
    export type ResponseBody = RoleDto;
  }

  /**
   * No description
   * @tags Role
   * @name RoleList
   * @request GET:/tenant/{tenantId}/role
   */
  export namespace RoleList {
    export type RequestParams = {
      tenantId: string;
    };
    export type RequestQuery = {
      Cursor?: string;
      /** @format int32 */
      Count?: number;
      IsPrevious?: boolean;
    };
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = RoleDto[];
  }

  /**
   * No description
   * @tags Role
   * @name RoleDetail
   * @request GET:/tenant/role/{roleId}
   */
  export namespace RoleDetail {
    export type RequestParams = {
      roleId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = RoleDto;
  }

  /**
   * No description
   * @tags Role
   * @name RoleUpdate
   * @request PUT:/tenant/role/{roleId}
   */
  export namespace RoleUpdate {
    export type RequestParams = {
      roleId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = UpdateRoleCommand;
    export type RequestHeaders = {};
    export type ResponseBody = void;
  }

  /**
   * No description
   * @tags Role
   * @name RoleDelete
   * @request DELETE:/tenant/role/{roleId}
   */
  export namespace RoleDelete {
    export type RequestParams = {
      roleId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = void;
  }

  /**
   * No description
   * @tags Role
   * @name RolePermissionCreate
   * @request POST:/tenant/role/{roleId}/permission/{permission}
   */
  export namespace RolePermissionCreate {
    export type RequestParams = {
      permission: string;
      roleId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = RoleDto;
  }

  /**
   * No description
   * @tags Role
   * @name RolePermissionDelete
   * @request DELETE:/tenant/role/{roleId}/permission/{permission}
   */
  export namespace RolePermissionDelete {
    export type RequestParams = {
      permission: string;
      roleId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = void;
  }

  /**
   * No description
   * @tags Role
   * @name RoleUserCreate
   * @request POST:/tenant/role/{roleId}/user/{userId}
   */
  export namespace RoleUserCreate {
    export type RequestParams = {
      userId: string;
      roleId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = RoleDto;
  }

  /**
   * No description
   * @tags Role
   * @name RoleUserDelete
   * @request DELETE:/tenant/role/{roleId}/user/{userId}
   */
  export namespace RoleUserDelete {
    export type RequestParams = {
      userId: string;
      roleId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = void;
  }

  /**
   * No description
   * @tags Role
   * @name RoleUserDetail
   * @request GET:/tenant/{tenantId}/role/user/{userId}
   */
  export namespace RoleUserDetail {
    export type RequestParams = {
      tenantId: string;
      userId: string;
    };
    export type RequestQuery = {
      Cursor?: string;
      /** @format int32 */
      Count: number;
    };
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = RoleDto[];
  }

  /**
   * No description
   * @tags Role
   * @name RolePermissionList
   * @request GET:/tenant/role/permission
   */
  export namespace RolePermissionList {
    export type RequestParams = {};
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = string[];
  }

  /**
   * No description
   * @tags Role
   * @name RolePermissionMeList
   * @request GET:/tenant/{tenantId}/role/permission/me
   */
  export namespace RolePermissionMeList {
    export type RequestParams = {
      tenantId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = string[];
  }

  /**
   * No description
   * @tags Tenant
   * @name TenantCreate
   * @request POST:/tenant
   */
  export namespace TenantCreate {
    export type RequestParams = {};
    export type RequestQuery = {};
    export type RequestBody = CreateTenantCommand;
    export type RequestHeaders = {};
    export type ResponseBody = TenantDto;
  }

  /**
   * No description
   * @tags Tenant
   * @name TenantList
   * @request GET:/tenant
   */
  export namespace TenantList {
    export type RequestParams = {};
    export type RequestQuery = {
      Cursor?: string;
      /** @format int32 */
      Count?: number;
      IsPrevious?: boolean;
    };
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = TenantDto[];
  }

  /**
   * No description
   * @tags Tenant
   * @name GetTenant
   * @request GET:/tenant/me
   */
  export namespace GetTenant {
    export type RequestParams = {};
    export type RequestQuery = {
      Cursor?: string;
      /** @format int32 */
      Count: number;
      IsPrevious: boolean;
    };
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = TenantDto[];
  }

  /**
   * No description
   * @tags Tenant
   * @name TenantDetail
   * @request GET:/tenant/{tenantId}
   */
  export namespace TenantDetail {
    export type RequestParams = {
      tenantId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = TenantDto;
  }

  /**
   * No description
   * @tags Tenant
   * @name TenantUpdate
   * @request PUT:/tenant/{tenantId}
   */
  export namespace TenantUpdate {
    export type RequestParams = {
      tenantId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = UpdateTenantCommand;
    export type RequestHeaders = {};
    export type ResponseBody = void;
  }

  /**
   * No description
   * @tags Tenant
   * @name TenantDelete
   * @request DELETE:/tenant/{tenantId}
   */
  export namespace TenantDelete {
    export type RequestParams = {
      tenantId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = void;
  }

  /**
   * No description
   * @tags Tenant
   * @name UserMapDelete
   * @request DELETE:/tenant/{tenantId}/user-map/{userId}
   */
  export namespace UserMapDelete {
    export type RequestParams = {
      tenantId: string;
      userId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = void;
  }

  /**
   * No description
   * @tags User
   * @name UserCreate
   * @request POST:/tenant/{tenantId}/user
   */
  export namespace UserCreate {
    export type RequestParams = {
      tenantId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = UserRegisterCommand;
    export type RequestHeaders = {};
    export type ResponseBody = UserDto;
  }

  /**
   * No description
   * @tags User
   * @name UserList
   * @request GET:/tenant/{tenantId}/user
   */
  export namespace UserList {
    export type RequestParams = {
      tenantId: string;
    };
    export type RequestQuery = {
      Cursor?: string;
      /** @format int32 */
      Count: number;
      IsPrevious: boolean;
    };
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = UserDto[];
  }

  /**
   * No description
   * @tags User
   * @name UserDetail
   * @request GET:/tenant/{tenantId}/user/{userId}
   */
  export namespace UserDetail {
    export type RequestParams = {
      tenantId: string;
      userId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = UserDto;
  }

  /**
   * No description
   * @tags User
   * @name UserUpdate
   * @request PUT:/tenant/{tenantId}/user/{userId}
   */
  export namespace UserUpdate {
    export type RequestParams = {
      tenantId: string;
      userId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = UpdateUserCommand;
    export type RequestHeaders = {};
    export type ResponseBody = UserDto;
  }

  /**
   * No description
   * @tags User
   * @name UserDelete
   * @request DELETE:/tenant/{tenantId}/user/{userId}
   */
  export namespace UserDelete {
    export type RequestParams = {
      tenantId: string;
      userId: string;
    };
    export type RequestQuery = {};
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = UserDto;
  }

  /**
   * No description
   * @tags User
   * @name OrganizationUserList
   * @request GET:/tenant/organization/{orgId}/user
   */
  export namespace OrganizationUserList {
    export type RequestParams = {
      orgId: string;
    };
    export type RequestQuery = {
      Cursor?: string;
      /** @format int32 */
      Count: number;
      IsPrevious: boolean;
    };
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = UserDto[];
  }

  /**
   * No description
   * @tags User
   * @name OrganizationUserNotInList
   * @request GET:/tenant/organization/{orgId}/user/not-in
   */
  export namespace OrganizationUserNotInList {
    export type RequestParams = {
      orgId: string;
    };
    export type RequestQuery = {
      Cursor?: string;
      /** @format int32 */
      Count: number;
      IsPrevious: boolean;
    };
    export type RequestBody = never;
    export type RequestHeaders = {};
    export type ResponseBody = UserDto[];
  }
}

import type {
  AxiosInstance,
  AxiosRequestConfig,
  AxiosResponse,
  HeadersDefaults,
  ResponseType,
} from "axios";
import axios from "axios";

export type QueryParamsType = Record<string | number, any>;

export interface FullRequestParams
  extends Omit<AxiosRequestConfig, "data" | "params" | "url" | "responseType"> {
  /** set parameter to `true` for call `securityWorker` for this request */
  secure?: boolean;
  /** request path */
  path: string;
  /** content type of request body */
  type?: ContentType;
  /** query params */
  query?: QueryParamsType;
  /** format of response (i.e. response.json() -> format: "json") */
  format?: ResponseType;
  /** request body */
  body?: unknown;
}

export type RequestParams = Omit<
  FullRequestParams,
  "body" | "method" | "query" | "path"
>;

export interface ApiConfig<SecurityDataType = unknown>
  extends Omit<AxiosRequestConfig, "data" | "cancelToken"> {
  securityWorker?: (
    securityData: SecurityDataType | null,
  ) => Promise<AxiosRequestConfig | void> | AxiosRequestConfig | void;
  secure?: boolean;
  format?: ResponseType;
}

export enum ContentType {
  Json = "application/json",
  JsonApi = "application/vnd.api+json",
  FormData = "multipart/form-data",
  UrlEncoded = "application/x-www-form-urlencoded",
  Text = "text/plain",
}

export class HttpClient<SecurityDataType = unknown> {
  public instance: AxiosInstance;
  private securityData: SecurityDataType | null = null;
  private securityWorker?: ApiConfig<SecurityDataType>["securityWorker"];
  private secure?: boolean;
  private format?: ResponseType;

  constructor({
    securityWorker,
    secure,
    format,
    ...axiosConfig
  }: ApiConfig<SecurityDataType> = {}) {
    this.instance = axios.create({
      ...axiosConfig,
      baseURL: axiosConfig.baseURL || "",
    });
    this.secure = secure;
    this.format = format;
    this.securityWorker = securityWorker;
  }

  public setSecurityData = (data: SecurityDataType | null) => {
    this.securityData = data;
  };

  protected mergeRequestParams(
    params1: AxiosRequestConfig,
    params2?: AxiosRequestConfig,
  ): AxiosRequestConfig {
    const method = params1.method || (params2 && params2.method);

    return {
      ...this.instance.defaults,
      ...params1,
      ...(params2 || {}),
      headers: {
        ...((method &&
          this.instance.defaults.headers[
            method.toLowerCase() as keyof HeadersDefaults
          ]) ||
          {}),
        ...(params1.headers || {}),
        ...((params2 && params2.headers) || {}),
      },
    };
  }

  protected stringifyFormItem(formItem: unknown) {
    if (typeof formItem === "object" && formItem !== null) {
      return JSON.stringify(formItem);
    } else {
      return `${formItem}`;
    }
  }

  protected createFormData(input: Record<string, unknown>): FormData {
    if (input instanceof FormData) {
      return input;
    }
    return Object.keys(input || {}).reduce((formData, key) => {
      const property = input[key];
      const propertyContent: any[] =
        property instanceof Array ? property : [property];

      for (const formItem of propertyContent) {
        const isFileType = formItem instanceof Blob || formItem instanceof File;
        formData.append(
          key,
          isFileType ? formItem : this.stringifyFormItem(formItem),
        );
      }

      return formData;
    }, new FormData());
  }

  public request = async <T = any, _E = any>({
    secure,
    path,
    type,
    query,
    format,
    body,
    ...params
  }: FullRequestParams): Promise<AxiosResponse<T>> => {
    const secureParams =
      ((typeof secure === "boolean" ? secure : this.secure) &&
        this.securityWorker &&
        (await this.securityWorker(this.securityData))) ||
      {};
    const requestParams = this.mergeRequestParams(params, secureParams);
    const responseFormat = format || this.format || undefined;

    if (
      type === ContentType.FormData &&
      body &&
      body !== null &&
      typeof body === "object"
    ) {
      body = this.createFormData(body as Record<string, unknown>);
    }

    if (
      type === ContentType.Text &&
      body &&
      body !== null &&
      typeof body !== "string"
    ) {
      body = JSON.stringify(body);
    }

    return this.instance.request({
      ...requestParams,
      headers: {
        ...(requestParams.headers || {}),
        ...(type ? { "Content-Type": type } : {}),
      },
      params: query,
      responseType: responseFormat,
      data: body,
      url: path,
    });
  };
}

/**
 * @title FastQuiz.API
 * @version 1.0
 */
export class Api<
  SecurityDataType extends unknown,
> extends HttpClient<SecurityDataType> {
  auth = {
    /**
     * No description
     *
     * @tags Auth
     * @name RegisterCreate
     * @request POST:/auth/register
     */
    registerCreate: (data: UserRegisterCommand, params: RequestParams = {}) =>
      this.request<AuthDto, ProblemDetails>({
        path: `/auth/register`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Auth
     * @name LoginCreate
     * @request POST:/auth/login
     */
    loginCreate: (data: UserLoginCommand, params: RequestParams = {}) =>
      this.request<AuthDto, ProblemDetails | void>({
        path: `/auth/login`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Auth
     * @name ProfileList
     * @request GET:/auth/profile
     */
    profileList: (params: RequestParams = {}) =>
      this.request<UserDto, ProblemDetails | void>({
        path: `/auth/profile`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Auth
     * @name PutAuth
     * @request PUT:/auth/me
     */
    putAuth: (data: UpdateUserCommand, params: RequestParams = {}) =>
      this.request<UserDto, ProblemDetails>({
        path: `/auth/me`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),
  };
  tenant = {
    /**
     * No description
     *
     * @tags Organization
     * @name OrganizationCreate
     * @request POST:/tenant/{tenantId}/organization
     */
    organizationCreate: (
      tenantId: string,
      data: CreateOrganizationCommand,
      params: RequestParams = {},
    ) =>
      this.request<OrganizationDto, ProblemDetails>({
        path: `/tenant/${tenantId}/organization`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Organization
     * @name OrganizationList
     * @request GET:/tenant/{tenantId}/organization
     */
    organizationList: (
      tenantId: string,
      query: {
        Cursor?: string;
        /** @format int32 */
        Count: number;
        IsPrevious: boolean;
      },
      params: RequestParams = {},
    ) =>
      this.request<OrganizationDto[], ProblemDetails | void>({
        path: `/tenant/${tenantId}/organization`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Organization
     * @name OrganizationDetail
     * @request GET:/tenant/{tenantId}/organization/{parentId}
     */
    organizationDetail: (
      tenantId: string,
      parentId: string,
      query: {
        Cursor?: string;
        /** @format int32 */
        Count: number;
        IsPrevious: boolean;
      },
      params: RequestParams = {},
    ) =>
      this.request<OrganizationDto[], ProblemDetails | void>({
        path: `/tenant/${tenantId}/organization/${parentId}`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Organization
     * @name OrganizationDetail2
     * @request GET:/tenant/organization/{orgId}
     * @originalName organizationDetail
     * @duplicate
     */
    organizationDetail2: (orgId: string, params: RequestParams = {}) =>
      this.request<OrganizationDto, ProblemDetails | void>({
        path: `/tenant/organization/${orgId}`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Organization
     * @name OrganizationUpdate
     * @request PUT:/tenant/organization/{orgId}
     */
    organizationUpdate: (
      orgId: string,
      data: UpdateOrganizationCommand,
      params: RequestParams = {},
    ) =>
      this.request<void, ProblemDetails | void>({
        path: `/tenant/organization/${orgId}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Organization
     * @name OrganizationDelete
     * @request DELETE:/tenant/organization/{orgId}
     */
    organizationDelete: (orgId: string, params: RequestParams = {}) =>
      this.request<void, ProblemDetails | void>({
        path: `/tenant/organization/${orgId}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Organization
     * @name OrganizationUserCreate
     * @request POST:/tenant/organization/{orgId}/user/{userId}
     */
    organizationUserCreate: (
      orgId: string,
      userId: string,
      params: RequestParams = {},
    ) =>
      this.request<OrganizationDto, ProblemDetails>({
        path: `/tenant/organization/${orgId}/user/${userId}`,
        method: "POST",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Organization
     * @name OrganizationUserDelete
     * @request DELETE:/tenant/organization{orgId}/user/{userId}
     */
    organizationUserDelete: (
      orgId: string,
      userId: string,
      params: RequestParams = {},
    ) =>
      this.request<void, ProblemDetails | void>({
        path: `/tenant/organization${orgId}/user/${userId}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuestionGroup
     * @name QuestionGroupCreate
     * @request POST:/tenant/{tenantId}/question-group
     */
    questionGroupCreate: (
      tenantId: string,
      data: CreateQuestionGroupCommand,
      params: RequestParams = {},
    ) =>
      this.request<QuestionGroupDto, ProblemDetails>({
        path: `/tenant/${tenantId}/question-group`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuestionGroup
     * @name QuestionGroupList
     * @request GET:/tenant/{tenantId}/question-group
     */
    questionGroupList: (
      tenantId: string,
      query?: {
        Cursor?: string;
        /** @format int32 */
        Count?: number;
        IsPrevious?: boolean;
        NamePrefix?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<QuestionGroupDto[], ProblemDetails | void>({
        path: `/tenant/${tenantId}/question-group`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuestionGroup
     * @name QuestionGroupBatchDeleteCreate
     * @request POST:/tenant/{tenantId}/question-group/batch-delete
     */
    questionGroupBatchDeleteCreate: (
      tenantId: string,
      data: DeleteQuestionGroupCommand,
      params: RequestParams = {},
    ) =>
      this.request<void, ProblemDetails | void>({
        path: `/tenant/${tenantId}/question-group/batch-delete`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuestionGroup
     * @name QuestionGroupDetail
     * @request GET:/tenant/{tenantId}/question-group/{groupId}
     */
    questionGroupDetail: (
      tenantId: string,
      groupId: string,
      params: RequestParams = {},
    ) =>
      this.request<QuestionGroupDto, ProblemDetails | void>({
        path: `/tenant/${tenantId}/question-group/${groupId}`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuestionGroup
     * @name QuestionGroupUpdate
     * @request PUT:/tenant/{tenantId}/question-group/{groupId}
     */
    questionGroupUpdate: (
      tenantId: string,
      groupId: string,
      data: UpdateQuestionGroupCommand,
      params: RequestParams = {},
    ) =>
      this.request<void, ProblemDetails | void>({
        path: `/tenant/${tenantId}/question-group/${groupId}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuestionGroup
     * @name QuestionGroupDelete
     * @request DELETE:/tenant/{tenantId}/question-group/{groupId}
     */
    questionGroupDelete: (
      tenantId: string,
      groupId: string,
      params: RequestParams = {},
    ) =>
      this.request<void, ProblemDetails | void>({
        path: `/tenant/${tenantId}/question-group/${groupId}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuestionGroup
     * @name QuestionGroupCountList
     * @request GET:/tenant/{tenantId}/question-group/{groupId}/count
     */
    questionGroupCountList: (
      tenantId: string,
      groupId: string,
      query: {
        Type: QuestionType;
        /** @format int32 */
        Level: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<number, ProblemDetails | void>({
        path: `/tenant/${tenantId}/question-group/${groupId}/count`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuestionGroup
     * @name QuestionGroupStatisticsList
     * @request GET:/tenant/{tenantId}/question-group/{groupId}/statistics
     */
    questionGroupStatisticsList: (
      tenantId: string,
      groupId: string,
      params: RequestParams = {},
    ) =>
      this.request<QuestionGroupStatisticsDto, ProblemDetails | void>({
        path: `/tenant/${tenantId}/question-group/${groupId}/statistics`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuestionGroup
     * @name QuestionGroupQuestionCreate
     * @request POST:/tenant/{tenantId}/question-group/{groupId}/question
     */
    questionGroupQuestionCreate: (
      tenantId: string,
      groupId: string,
      data: CreateQuestionCommand,
      params: RequestParams = {},
    ) =>
      this.request<QuestionDto, ProblemDetails>({
        path: `/tenant/${tenantId}/question-group/${groupId}/question`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuestionGroup
     * @name QuestionGroupQuestionDetail
     * @request GET:/tenant/{tenantId}/question-group/question/{questionId}
     */
    questionGroupQuestionDetail: (
      tenantId: string,
      questionId: string,
      params: RequestParams = {},
    ) =>
      this.request<QuestionDto, ProblemDetails>({
        path: `/tenant/${tenantId}/question-group/question/${questionId}`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuestionGroup
     * @name QuestionGroupQuestionUpdate
     * @request PUT:/tenant/{tenantId}/question-group/question/{questionId}
     */
    questionGroupQuestionUpdate: (
      tenantId: string,
      questionId: string,
      data: UpdateQuestionCommand,
      params: RequestParams = {},
    ) =>
      this.request<void, ProblemDetails>({
        path: `/tenant/${tenantId}/question-group/question/${questionId}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuestionGroup
     * @name QuestionGroupQuestionDelete
     * @request DELETE:/tenant/{tenantId}/question-group/question/{questionId}
     */
    questionGroupQuestionDelete: (
      tenantId: string,
      questionId: string,
      params: RequestParams = {},
    ) =>
      this.request<void, ProblemDetails>({
        path: `/tenant/${tenantId}/question-group/question/${questionId}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuestionGroup
     * @name QuestionGroupQuestionList
     * @request GET:/tenant/{tenantId}/question-group/{questionGroupId}/question
     */
    questionGroupQuestionList: (
      tenantId: string,
      questionGroupId: string,
      query?: {
        Cursor?: string;
        Type?: QuestionType;
        /** @format int32 */
        Level?: number;
        /** @format int32 */
        Count?: number;
        IsPrevious?: boolean;
      },
      params: RequestParams = {},
    ) =>
      this.request<QuestionDto[], ProblemDetails | void>({
        path: `/tenant/${tenantId}/question-group/${questionGroupId}/question`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizAttempt
     * @name QuizTemplateQuizAttemptCreate
     * @request POST:/tenant/{tenantId}/quiz-template/{quizTemplateId}/quiz-attempt
     */
    quizTemplateQuizAttemptCreate: (
      tenantId: string,
      quizTemplateId: string,
      params: RequestParams = {},
    ) =>
      this.request<QuizAttempDto, ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template/${quizTemplateId}/quiz-attempt`,
        method: "POST",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizAttempt
     * @name QuizTemplateQuizAttemptList
     * @request GET:/tenant/{tenantId}/quiz-template/{quizTemplateId}/quiz-attempt
     */
    quizTemplateQuizAttemptList: (
      tenantId: string,
      quizTemplateId: string,
      query?: {
        Cursor?: string;
        /** @format int32 */
        Count?: number;
        IsPrevious?: boolean;
      },
      params: RequestParams = {},
    ) =>
      this.request<QuizAttempDto[], ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template/${quizTemplateId}/quiz-attempt`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizAttempt
     * @name QuizTemplateQuizAttemptBatchCreate
     * @request POST:/tenant/{tenantId}/quiz-template/{quizTemplateId}/quiz-attempt/batch
     */
    quizTemplateQuizAttemptBatchCreate: (
      tenantId: string,
      quizTemplateId: string,
      data: CreateQuizAttemptCommand[],
      params: RequestParams = {},
    ) =>
      this.request<void, ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template/${quizTemplateId}/quiz-attempt/batch`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizAttempt
     * @name QuizTemplateQuizAttemptRecordsList
     * @request GET:/tenant/{tenantId}/quiz-template/{quizTemplateId}/quiz-attempt/records
     */
    quizTemplateQuizAttemptRecordsList: (
      tenantId: string,
      quizTemplateId: string,
      query?: {
        OnlyBest?: boolean;
        Cursor?: string;
        /** @format int32 */
        Count?: number;
        IsPrevious?: boolean;
      },
      params: RequestParams = {},
    ) =>
      this.request<QuizAttempDto[], ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template/${quizTemplateId}/quiz-attempt/records`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizAttempt
     * @name QuizTemplateQuizAttemptMeList
     * @request GET:/tenant/{tenantId}/quiz-template/quiz-attempt/me
     */
    quizTemplateQuizAttemptMeList: (
      tenantId: string,
      query?: {
        Cursor?: string;
        /** @format int32 */
        Count?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<QuizAttempDto[], ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template/quiz-attempt/me`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizAttempt
     * @name QuizTemplateQuizAttemptMeTemplateDetail
     * @request GET:/tenant/{tenantId}/quiz-template/quiz-attempt/me/template/{quizTemplateId}
     */
    quizTemplateQuizAttemptMeTemplateDetail: (
      tenantId: string,
      quizTemplateId: string,
      query?: {
        Cursor?: string;
        /** @format int32 */
        Count?: number;
        IsPrevious?: boolean;
      },
      params: RequestParams = {},
    ) =>
      this.request<QuizAttempDto[], ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template/quiz-attempt/me/template/${quizTemplateId}`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizAttempt
     * @name QuizTemplateQuizAttemptUserRecordsList
     * @request GET:/tenant/{tenantId}/quiz-template/quiz-attempt/user/{userId}/records
     */
    quizTemplateQuizAttemptUserRecordsList: (
      tenantId: string,
      userId: string,
      query?: {
        TemplateId?: string;
        OnlyBest?: boolean;
        Cursor?: string;
        /** @format int32 */
        Count?: number;
        IsPrevious?: boolean;
      },
      params: RequestParams = {},
    ) =>
      this.request<QuizAttempDto[], ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template/quiz-attempt/user/${userId}/records`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizAttempt
     * @name QuizTemplateQuizAttemptDetail
     * @request GET:/tenant/{tenantId}/quiz-template/quiz-attempt/{quizAttemptId}
     */
    quizTemplateQuizAttemptDetail: (
      tenantId: string,
      quizAttemptId: string,
      params: RequestParams = {},
    ) =>
      this.request<QuizAttempDto, ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template/quiz-attempt/${quizAttemptId}`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizAttempt
     * @name QuizTemplateQuizAttemptUpdate
     * @request PUT:/tenant/{tenantId}/quiz-template/quiz-attempt/{quizAttemptId}
     */
    quizTemplateQuizAttemptUpdate: (
      tenantId: string,
      quizAttemptId: string,
      query: {
        status: QuizAttemptStatus;
      },
      params: RequestParams = {},
    ) =>
      this.request<void, ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template/quiz-attempt/${quizAttemptId}`,
        method: "PUT",
        query: query,
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizAttempt
     * @name QuizTemplateQuizAttemptQuestionList
     * @request GET:/tenant/{tenantId}/quiz-template/quiz-attempt/{quizAttemptId}/question
     */
    quizTemplateQuizAttemptQuestionList: (
      tenantId: string,
      quizAttemptId: string,
      query?: {
        Cursor?: string;
        /** @format int32 */
        Count?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<QuestionRecordWithoutAnswerDto[], ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template/quiz-attempt/${quizAttemptId}/question`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizAttempt
     * @name QuizTemplateQuizAttemptQuestionWithAnswerList
     * @request GET:/tenant/{tenantId}/quiz-template/quiz-attempt/{quizAttemptId}/question/with-answer
     */
    quizTemplateQuizAttemptQuestionWithAnswerList: (
      tenantId: string,
      quizAttemptId: string,
      query?: {
        Cursor?: string;
        /** @format int32 */
        Count?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<QuestionRecordWithAnswerDto[], ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template/quiz-attempt/${quizAttemptId}/question/with-answer`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizAttempt
     * @name QuizTemplateQuizAttemptQuestionUpdate
     * @request PUT:/tenant/{tenantId}/quiz-template/quiz-attempt/question/{questionId}
     */
    quizTemplateQuizAttemptQuestionUpdate: (
      tenantId: string,
      questionId: string,
      data: SetAnswerRecordCommand[],
      params: RequestParams = {},
    ) =>
      this.request<void, ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template/quiz-attempt/question/${questionId}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizAttempt
     * @name QuizTemplateQuizAttemptQuestionStatusUpdate
     * @request PUT:/tenant/{tenantId}/quiz-template/quiz-attempt/question/{questionId}/status/{isCorrect}
     */
    quizTemplateQuizAttemptQuestionStatusUpdate: (
      tenantId: string,
      questionId: string,
      isCorrect: boolean,
      params: RequestParams = {},
    ) =>
      this.request<void, ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template/quiz-attempt/question/${questionId}/status/${isCorrect}`,
        method: "PUT",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizSnapshot
     * @name QuizTemplateQuizSnapshotCreate
     * @request POST:/tenant/{tenantId}/quiz-template/{quizTemplateId}/quiz-snapshot
     */
    quizTemplateQuizSnapshotCreate: (
      tenantId: string,
      quizTemplateId: string,
      data: CreateQuizSnapshotCommand,
      params: RequestParams = {},
    ) =>
      this.request<void, ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template/${quizTemplateId}/quiz-snapshot`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizSnapshot
     * @name QuizTemplateQuizSnapshotList
     * @request GET:/tenant/{tenantId}/quiz-template/{quizTemplateId}/quiz-snapshot
     */
    quizTemplateQuizSnapshotList: (
      tenantId: string,
      quizTemplateId: string,
      query?: {
        Cursor?: string;
        /** @format int32 */
        Count?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<QuizSnapshotDto[], ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template/${quizTemplateId}/quiz-snapshot`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizSnapshot
     * @name QuizTemplateQuizSnapshotDetail
     * @request GET:/tenant/{tenantId}/quiz-template/quiz-snapshot/{quizSnapshotId}
     */
    quizTemplateQuizSnapshotDetail: (
      tenantId: string,
      quizSnapshotId: string,
      params: RequestParams = {},
    ) =>
      this.request<QuizSnapshotDto, ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template/quiz-snapshot/${quizSnapshotId}`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizSnapshot
     * @name QuizTemplateQuizSnapshotDelete
     * @request DELETE:/tenant/{tenantId}/quiz-template/quiz-snapshot/{quizSnapshotId}
     */
    quizTemplateQuizSnapshotDelete: (
      tenantId: string,
      quizSnapshotId: string,
      params: RequestParams = {},
    ) =>
      this.request<void, ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template/quiz-snapshot/${quizSnapshotId}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizSnapshot
     * @name QuizTemplateQuizSnapshotQuestionSnapshotList
     * @request GET:/tenant/{tenantId}/quiz-template/quiz-snapshot/{quizSnapshotId}/question-snapshot
     */
    quizTemplateQuizSnapshotQuestionSnapshotList: (
      tenantId: string,
      quizSnapshotId: string,
      query?: {
        Cursor?: string;
        /** @format int32 */
        Count?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<QuestionSnapshotDto[], ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template/quiz-snapshot/${quizSnapshotId}/question-snapshot`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizTemplate
     * @name QuizTemplateCreate
     * @request POST:/tenant/{tenantId}/quiz-template
     */
    quizTemplateCreate: (
      tenantId: string,
      data: CreateQuizTemplateCommand,
      params: RequestParams = {},
    ) =>
      this.request<QuizTemplateDto, ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizTemplate
     * @name QuizTemplateList
     * @request GET:/tenant/{tenantId}/quiz-template
     */
    quizTemplateList: (
      tenantId: string,
      query?: {
        Cursor?: string;
        /** @format int32 */
        Count?: number;
        IsPrevious?: boolean;
      },
      params: RequestParams = {},
    ) =>
      this.request<QuizTemplateDto[], ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizTemplate
     * @name QuizTemplatePublishedList
     * @request GET:/tenant/{tenantId}/quiz-template/published
     */
    quizTemplatePublishedList: (
      tenantId: string,
      query?: {
        Cursor?: string;
        /** @format int32 */
        Count?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<QuizTemplateDto[], ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template/published`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizTemplate
     * @name QuizTemplateDetail
     * @request GET:/tenant/{tenantId}/quiz-template/{quizTemplateId}
     */
    quizTemplateDetail: (
      tenantId: string,
      quizTemplateId: string,
      params: RequestParams = {},
    ) =>
      this.request<QuizTemplateDto, ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template/${quizTemplateId}`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizTemplate
     * @name QuizTemplateUpdate
     * @request PUT:/tenant/{tenantId}/quiz-template/{quizTemplateId}
     */
    quizTemplateUpdate: (
      tenantId: string,
      quizTemplateId: string,
      data: UpdateQuizTemplateCommand,
      params: RequestParams = {},
    ) =>
      this.request<void, ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template/${quizTemplateId}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizTemplate
     * @name QuizTemplateDelete
     * @request DELETE:/tenant/{tenantId}/quiz-template/{quizTemplateId}
     */
    quizTemplateDelete: (
      tenantId: string,
      quizTemplateId: string,
      params: RequestParams = {},
    ) =>
      this.request<void, ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template/${quizTemplateId}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizTemplate
     * @name QuizTemplatePublishedUpdate
     * @request PUT:/tenant/{tenantId}/quiz-template/{quizTemplateId}/published
     */
    quizTemplatePublishedUpdate: (
      tenantId: string,
      quizTemplateId: string,
      data: UpdateQuizTemplatePublishedCommand,
      params: RequestParams = {},
    ) =>
      this.request<void, ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template/${quizTemplateId}/published`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizTemplate
     * @name QuizTemplateRuleCreate
     * @request POST:/tenant/{tenantId}/quiz-template/{quizTemplateId}/rule
     */
    quizTemplateRuleCreate: (
      tenantId: string,
      quizTemplateId: string,
      data: CreateQuestionSelectionRuleCommand,
      params: RequestParams = {},
    ) =>
      this.request<QuestionSelectionRuleDto, ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template/${quizTemplateId}/rule`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizTemplate
     * @name QuizTemplateRuleList
     * @request GET:/tenant/{tenantId}/quiz-template/{quizTemplateId}/rule
     */
    quizTemplateRuleList: (
      tenantId: string,
      quizTemplateId: string,
      query?: {
        Cursor?: string;
        /** @format int32 */
        Count?: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<QuestionSelectionRuleDto[], ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template/${quizTemplateId}/rule`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizTemplate
     * @name QuizTemplateRuleDetail
     * @request GET:/tenant/{tenantId}/quiz-template/rule/{ruleId}
     */
    quizTemplateRuleDetail: (
      tenantId: string,
      ruleId: string,
      params: RequestParams = {},
    ) =>
      this.request<QuestionSelectionRuleDto, ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template/rule/${ruleId}`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizTemplate
     * @name QuizTemplateRuleUpdate
     * @request PUT:/tenant/{tenantId}/quiz-template/rule/{ruleId}
     */
    quizTemplateRuleUpdate: (
      tenantId: string,
      ruleId: string,
      data: UpdateQuestionSelectionRuleCommand,
      params: RequestParams = {},
    ) =>
      this.request<void, ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template/rule/${ruleId}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags QuizTemplate
     * @name QuizTemplateRuleDelete
     * @request DELETE:/tenant/{tenantId}/quiz-template/rule/{ruleId}
     */
    quizTemplateRuleDelete: (
      tenantId: string,
      ruleId: string,
      params: RequestParams = {},
    ) =>
      this.request<void, ProblemDetails>({
        path: `/tenant/${tenantId}/quiz-template/rule/${ruleId}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Role
     * @name RoleCreate
     * @request POST:/tenant/{tenantId}/role
     */
    roleCreate: (
      tenantId: string,
      data: CreateRoleCommand,
      params: RequestParams = {},
    ) =>
      this.request<RoleDto, ProblemDetails>({
        path: `/tenant/${tenantId}/role`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Role
     * @name RoleList
     * @request GET:/tenant/{tenantId}/role
     */
    roleList: (
      tenantId: string,
      query?: {
        Cursor?: string;
        /** @format int32 */
        Count?: number;
        IsPrevious?: boolean;
      },
      params: RequestParams = {},
    ) =>
      this.request<RoleDto[], ProblemDetails | void>({
        path: `/tenant/${tenantId}/role`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Role
     * @name RoleDetail
     * @request GET:/tenant/role/{roleId}
     */
    roleDetail: (roleId: string, params: RequestParams = {}) =>
      this.request<RoleDto, ProblemDetails | void>({
        path: `/tenant/role/${roleId}`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Role
     * @name RoleUpdate
     * @request PUT:/tenant/role/{roleId}
     */
    roleUpdate: (
      roleId: string,
      data: UpdateRoleCommand,
      params: RequestParams = {},
    ) =>
      this.request<void, ProblemDetails | void>({
        path: `/tenant/role/${roleId}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Role
     * @name RoleDelete
     * @request DELETE:/tenant/role/{roleId}
     */
    roleDelete: (roleId: string, params: RequestParams = {}) =>
      this.request<void, ProblemDetails | void>({
        path: `/tenant/role/${roleId}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Role
     * @name RolePermissionCreate
     * @request POST:/tenant/role/{roleId}/permission/{permission}
     */
    rolePermissionCreate: (
      permission: string,
      roleId: string,
      params: RequestParams = {},
    ) =>
      this.request<RoleDto, ProblemDetails>({
        path: `/tenant/role/${roleId}/permission/${permission}`,
        method: "POST",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Role
     * @name RolePermissionDelete
     * @request DELETE:/tenant/role/{roleId}/permission/{permission}
     */
    rolePermissionDelete: (
      permission: string,
      roleId: string,
      params: RequestParams = {},
    ) =>
      this.request<void, ProblemDetails | void>({
        path: `/tenant/role/${roleId}/permission/${permission}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Role
     * @name RoleUserCreate
     * @request POST:/tenant/role/{roleId}/user/{userId}
     */
    roleUserCreate: (
      userId: string,
      roleId: string,
      params: RequestParams = {},
    ) =>
      this.request<RoleDto, ProblemDetails>({
        path: `/tenant/role/${roleId}/user/${userId}`,
        method: "POST",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Role
     * @name RoleUserDelete
     * @request DELETE:/tenant/role/{roleId}/user/{userId}
     */
    roleUserDelete: (
      userId: string,
      roleId: string,
      params: RequestParams = {},
    ) =>
      this.request<void, ProblemDetails | void>({
        path: `/tenant/role/${roleId}/user/${userId}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Role
     * @name RoleUserDetail
     * @request GET:/tenant/{tenantId}/role/user/{userId}
     */
    roleUserDetail: (
      tenantId: string,
      userId: string,
      query: {
        Cursor?: string;
        /** @format int32 */
        Count: number;
      },
      params: RequestParams = {},
    ) =>
      this.request<RoleDto[], ProblemDetails>({
        path: `/tenant/${tenantId}/role/user/${userId}`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Role
     * @name RolePermissionList
     * @request GET:/tenant/role/permission
     */
    rolePermissionList: (params: RequestParams = {}) =>
      this.request<string[], ProblemDetails>({
        path: `/tenant/role/permission`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Role
     * @name RolePermissionMeList
     * @request GET:/tenant/{tenantId}/role/permission/me
     */
    rolePermissionMeList: (tenantId: string, params: RequestParams = {}) =>
      this.request<string[], ProblemDetails>({
        path: `/tenant/${tenantId}/role/permission/me`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Tenant
     * @name TenantCreate
     * @request POST:/tenant
     */
    tenantCreate: (data: CreateTenantCommand, params: RequestParams = {}) =>
      this.request<TenantDto, ProblemDetails>({
        path: `/tenant`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Tenant
     * @name TenantList
     * @request GET:/tenant
     */
    tenantList: (
      query?: {
        Cursor?: string;
        /** @format int32 */
        Count?: number;
        IsPrevious?: boolean;
      },
      params: RequestParams = {},
    ) =>
      this.request<TenantDto[], ProblemDetails | void>({
        path: `/tenant`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Tenant
     * @name GetTenant
     * @request GET:/tenant/me
     */
    getTenant: (
      query: {
        Cursor?: string;
        /** @format int32 */
        Count: number;
        IsPrevious: boolean;
      },
      params: RequestParams = {},
    ) =>
      this.request<TenantDto[], ProblemDetails | void>({
        path: `/tenant/me`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Tenant
     * @name TenantDetail
     * @request GET:/tenant/{tenantId}
     */
    tenantDetail: (tenantId: string, params: RequestParams = {}) =>
      this.request<TenantDto, ProblemDetails | void>({
        path: `/tenant/${tenantId}`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Tenant
     * @name TenantUpdate
     * @request PUT:/tenant/{tenantId}
     */
    tenantUpdate: (
      tenantId: string,
      data: UpdateTenantCommand,
      params: RequestParams = {},
    ) =>
      this.request<void, ProblemDetails | void>({
        path: `/tenant/${tenantId}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Tenant
     * @name TenantDelete
     * @request DELETE:/tenant/{tenantId}
     */
    tenantDelete: (tenantId: string, params: RequestParams = {}) =>
      this.request<void, ProblemDetails | void>({
        path: `/tenant/${tenantId}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Tenant
     * @name UserMapDelete
     * @request DELETE:/tenant/{tenantId}/user-map/{userId}
     */
    userMapDelete: (
      tenantId: string,
      userId: string,
      params: RequestParams = {},
    ) =>
      this.request<void, ProblemDetails>({
        path: `/tenant/${tenantId}/user-map/${userId}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags User
     * @name UserCreate
     * @request POST:/tenant/{tenantId}/user
     */
    userCreate: (
      tenantId: string,
      data: UserRegisterCommand,
      params: RequestParams = {},
    ) =>
      this.request<UserDto, ProblemDetails>({
        path: `/tenant/${tenantId}/user`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags User
     * @name UserList
     * @request GET:/tenant/{tenantId}/user
     */
    userList: (
      tenantId: string,
      query: {
        Cursor?: string;
        /** @format int32 */
        Count: number;
        IsPrevious: boolean;
      },
      params: RequestParams = {},
    ) =>
      this.request<UserDto[], ProblemDetails>({
        path: `/tenant/${tenantId}/user`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags User
     * @name UserDetail
     * @request GET:/tenant/{tenantId}/user/{userId}
     */
    userDetail: (
      tenantId: string,
      userId: string,
      params: RequestParams = {},
    ) =>
      this.request<UserDto, ProblemDetails>({
        path: `/tenant/${tenantId}/user/${userId}`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags User
     * @name UserUpdate
     * @request PUT:/tenant/{tenantId}/user/{userId}
     */
    userUpdate: (
      tenantId: string,
      userId: string,
      data: UpdateUserCommand,
      params: RequestParams = {},
    ) =>
      this.request<UserDto, ProblemDetails>({
        path: `/tenant/${tenantId}/user/${userId}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags User
     * @name UserDelete
     * @request DELETE:/tenant/{tenantId}/user/{userId}
     */
    userDelete: (
      tenantId: string,
      userId: string,
      params: RequestParams = {},
    ) =>
      this.request<UserDto, ProblemDetails>({
        path: `/tenant/${tenantId}/user/${userId}`,
        method: "DELETE",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags User
     * @name OrganizationUserList
     * @request GET:/tenant/organization/{orgId}/user
     */
    organizationUserList: (
      orgId: string,
      query: {
        Cursor?: string;
        /** @format int32 */
        Count: number;
        IsPrevious: boolean;
      },
      params: RequestParams = {},
    ) =>
      this.request<UserDto[], ProblemDetails>({
        path: `/tenant/organization/${orgId}/user`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags User
     * @name OrganizationUserNotInList
     * @request GET:/tenant/organization/{orgId}/user/not-in
     */
    organizationUserNotInList: (
      orgId: string,
      query: {
        Cursor?: string;
        /** @format int32 */
        Count: number;
        IsPrevious: boolean;
      },
      params: RequestParams = {},
    ) =>
      this.request<UserDto[], ProblemDetails>({
        path: `/tenant/organization/${orgId}/user/not-in`,
        method: "GET",
        query: query,
        format: "json",
        ...params,
      }),
  };
}
